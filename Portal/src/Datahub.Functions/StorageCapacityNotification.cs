using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using MimeKit;
using MailKit.Net.Smtp;
using Azure.Storage.Queues;
using System.Text;

namespace Datahub.Functions;

public class StorageCapacityNotification
{
    private readonly ILogger<StorageCapacityNotification> _logger;
    private readonly AzureConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DatahubProjectDBContext _dbContext;

    public StorageCapacityNotification(ILoggerFactory loggerFactory, IConfiguration configuration, IHttpClientFactory httpClientFactory, DatahubProjectDBContext dbContext)
    {
        _logger = loggerFactory.CreateLogger<StorageCapacityNotification>();
        _config = new(configuration);
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
    }

    /// <summary>
    /// The ScheduleStorageCapacityValidation function runs hourly
    /// </summary>
    [Function("ScheduleStorageCapacityValidation")]
    public async Task RunScheduleStorageCapacityValidation([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
    {
        var queueClient = new QueueClient(_config.StorageQueueConnection, "storage-capacity");
        var rows = await _dbContext.Project_Resources2.Where(e => e.ResourceType == "storage").ToListAsync();
        foreach (var row in rows)
        {
            if (!string.IsNullOrEmpty(row.JsonContent))
            {
                var message = GetStorageAccountMessage(row);
                if (message is not null)
                {
                    queueClient.SendMessage(message);
                }
            }
        }
    }

    /// <summary>
    /// Storage Capacity Notification run every time a message is deposited in queue: 'storage-capacity'
    /// </summary>
    [Function("StorageCapacityNotification")]
    public async Task RunStorageCapacityNotification([QueueTrigger("storage-capacity", Connection = "datahub-storage-queue")] string queueItem)
    {
        // deserialize message
        var msg = DeserializeQueueMessage(queueItem);

        // verify message 
        if (msg is null || msg.ProjectId <= 0 || string.IsNullOrEmpty(msg.ResourceGroup) || string.IsNullOrEmpty(msg.StorageAccount))
        {
            _logger.LogError("Invalid queue message!");
            return;
        }

        // get the datahub project
        var project = await GetDatahubProject(msg.ProjectId);
        if (project is null)
        {
            _logger.LogError($"Function could not find project with ID ({msg.ProjectId}).");
            return;
        }

        // get the valid contacts
        var contactList = GetValidProjectContacts(project.Contact_List);
        if (contactList.Count == 0)
        {
            _logger.LogError($"Function could not find valid contacts for project ({project.Project_Acronym_CD}).");
            return;
        }

        // validate email smtp configuration...

        // obtain an access token
        var token = await TryGetAccessToken();
        if (token is null)
        {
            _logger.LogError("Function failed to obtain oauth token, check function config and permissions");
            return;
        }

        // get used capacity from Azure
        var usedCapacity = await GetStorageUsedCapacity(msg.ResourceGroup, msg.StorageAccount, token);
        if (usedCapacity is null)
        {
            _logger.LogInformation($"Cannot read storage {msg.StorageAccount}'s used Capacity.");
            return;
        }

        // get the last known storage capacity entry (or a new one)
        var (lastCapacity, isNew) = await GetDbProjectStorageCapacity(msg.ProjectId);

        // get max capacity from config
        double configMaxCapacity = GetConfigMaxCapacity();

        // check and notify the capacity usage
        await NotifyProjectCapacityUsage(contactList, lastCapacity, usedCapacity.Value, configMaxCapacity);

        // update the db
        await UpdateDbStorageCapacity(lastCapacity, isNew);
    }

    private string? GetStorageAccountMessage(Project_Resources2 row)
    {
        var content = JsonSerializer.Deserialize<Dictionary<string, string>>(row.JsonContent);

        if (content is null || !content.ContainsKey("resource_group") || !content.ContainsKey("storage_account"))
            return default;

        var msgObj = new StorageAccountMessage(row.ProjectId, content["resource_group"], content["storage_account"]); 
        var message = JsonSerializer.Serialize(msgObj, GetJsonSerializerOptions());

        return EncodeBase64(message);
    }    

    static string EncodeBase64(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    private async Task NotifyProjectCapacityUsage(List<string> contacts, Project_Storage_Capacity lastCapacity, double usedCapacity, double configMaxCapacity)
    {
        var nowUtc = DateTime.UtcNow;
        var capacityPerc = usedCapacity / configMaxCapacity;

        if (capacityPerc >= 0.5 && capacityPerc < 0.75)
        {
            // try notify 50% usage
            if (lastCapacity.NotifiedAt50 is null)
            {
                await NotifyProjectUsers(contacts, 50);
                lastCapacity.NotifiedAt50 = nowUtc;
            }
        }

        if (capacityPerc >= 0.75)
        {
            // try notify 75% usage
            if (lastCapacity.NotifiedAt75 is null)
            {
                await NotifyProjectUsers(contacts, 75);
                lastCapacity.NotifiedAt75 = nowUtc;
            }
        }

        // note: a good improvement would be to clean the notification dates once the capacity goes under the % mark

        lastCapacity.LastUpdated = nowUtc;
        lastCapacity.UsedCapacity = usedCapacity;
    }

    static List<string> GetValidProjectContacts(string? contacts)
    {
        var separators = " ;".ToCharArray();
        return (contacts ?? "")
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Where(IsValidEmail)
            .ToList();
    }

    static bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    }

    static StorageAccountMessage? DeserializeQueueMessage(string message)
    {
        return JsonSerializer.Deserialize<StorageAccountMessage>(message, GetJsonSerializerOptions());
    }

    static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    private async Task<Datahub_Project?> GetDatahubProject(int projectId)
    {
        return await _dbContext.Projects.FirstOrDefaultAsync(p => p.Project_ID == projectId);
    }

    private async Task NotifyProjectUsers(List<string> contacts, int perc)
    {
        try
        {
            // temp 
            if (contacts.Count == 0)
                return;

            using MimeMessage message = new();

            message.From.Add(new MailboxAddress("Datahub", _config.EmailSmtpFrom));
            message.To.AddRange(contacts.Select(email => new MailboxAddress(email, email)));

            message.Subject = $"Datahub storage capacity update / Mise à jour de la capacité de stockage Datahub";
            message.Body = new TextPart("plain")
            {
                Text = $"Notice, {perc}% of your Datahub Storage Capacity has been reached. \nNotez que {perc} % de votre capacité de stockage Datahub a été atteinte."
            };

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(_config.EmailSmtpHost, _config.EmailSmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(new System.Net.NetworkCredential(_config.EmailSmtpUser, _config.EmailSmtpPassword));

            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email notification failed!");
        }
    }

    private double GetConfigMaxCapacity()
    {
        return double.Parse(_config.MaxStorageCapacity);
    }

    private async Task<string?> TryGetAccessToken()
    {
        string? token = default;
        try
        {
            var authUtil = new AuthenticationUtils(_config, _httpClientFactory);
            token = await authUtil.GetAccessTokenAsync("https://management.core.windows.net/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain a token!");
        }
        return token;
    }

    private async Task<double?> GetStorageUsedCapacity(string resourceGroup, string storageAccount, string token)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var url = GetRequestUrl(resourceGroup, storageAccount, 2);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
        httpRequest.Headers.Add("authorization", $"Bearer {token}");

        var response = await httpClient.SendAsync(httpRequest);
        var responseContent = await response.Content.ReadAsStringAsync();

        var usageResponse = JsonSerializer.Deserialize<StorageUsedResponse>(responseContent);

        return FindCapacity(usageResponse);
    }

    private string GetRequestUrl(string resourceGroup, string storageAccount, int hours)
    {
        var timespan = GetTimespan(hours);
        var version = "2019-07-01";
        return $"{_config.ManagementUrl}/subscriptions/{_config.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccount}/providers/microsoft.Insights/metrics?timespan={timespan}&metricnames=UsedCapacity&aggregation=average&metricNamespace=microsoft.storage%2Fstorageaccounts&validatedimensions=false&api-version={version}";
    }

    private async Task UpdateDbStorageCapacity(Project_Storage_Capacity entity, bool isNew)
    {
        if (isNew)
        {
            _dbContext.Storage_Capacities.Add(entity);
        }
        else
        {
            _dbContext.Storage_Capacities.Update(entity);
        }
        await _dbContext.SaveChangesAsync();
    }

    private async Task<(Project_Storage_Capacity Entity, bool isNew)> GetDbProjectStorageCapacity(int projectId)
    {
        var capacity = await _dbContext.Storage_Capacities.FirstOrDefaultAsync(e => e.ProjectId == projectId && e.Type == Storage_Type.AzureGen2);
        var isNew = false;
        if (capacity is null)
        {
            capacity = new()
            {
                ProjectId = projectId,
                Type = Storage_Type.AzureGen2,
                LastUpdated = DateTime.UtcNow
            };
            isNew = true;
        }
        return (capacity, isNew);
    }

    static string GetTimespan(int hours)
    {
        var dt = DateTime.UtcNow;
        var format = "yyyy-MM-ddTHH:mm:00.000Z";
        return $"{dt.AddHours(-hours).ToString(format)}/{dt.ToString(format)}";
    }

    static double? FindCapacity(StorageUsedResponse? response)
    {
        if (response is null)
            return default;

        if (response?.value is null || response.value.Count == 0)
            return default;

        var value = response.value[0];
        if (value.errorCode != "Success" || value.timeseries is null || value.timeseries.Count == 0)
            return default;

        var timeseries = value.timeseries[0];
        if (timeseries is null || timeseries.data.Count == 0)
            return default;

        var data = timeseries.data.OrderByDescending(d => d.timeStamp).FirstOrDefault(d => d.average != 0);

        return data?.average ?? 0.0;
    }
}

#nullable disable

#region Response Model

class StorageUsedResponse
{
    public double cost { get; set; }
    //public string timespan { get; set; }
    public string interval { get; set; }
    public List<MetricValue> value { get; set; }
    //public string @namespace { get; set; }
    //public string resourceregion { get; set; }
}

class MetricValue
{
    //public string id { get; set; }
    //public string type { get; set; }
    //public ValueName name { get; set; }
    //public string displayDescription { get; set; }
    public string unit { get; set; }
    public List<Timeseries> timeseries { get; set; }
    public string errorCode { get; set; }
}

class Timeseries
{
    //public List<object> metadatavalues { get; set; }
    public List<TimeseriesValue> data { get; set; }
}

//class ValueName
//{
//    public string value { get; set; }
//    public string localizedValue { get; set; }
//}

public class TimeseriesValue
{
    public DateTime timeStamp { get; set; }
    public double average { get; set; }
}

#endregion

#region Input Message

record StorageAccountMessage(int ProjectId, string ResourceGroup, string StorageAccount);

#endregion

#nullable enable