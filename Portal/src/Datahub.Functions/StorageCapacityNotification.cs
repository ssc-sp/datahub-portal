using Azure.Storage.Queues;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Datahub.Functions;

public class StorageCapacityNotification
{
    private readonly ILogger<StorageCapacityNotification> _logger;
    private readonly AzureConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DatahubProjectDBContext _dbContext;
    private readonly AzureManagementService _azureManagementService;

    public StorageCapacityNotification(ILoggerFactory loggerFactory, IConfiguration configuration, IHttpClientFactory httpClientFactory, 
            DatahubProjectDBContext dbContext, AzureManagementService azureManagementService)
    {
        _logger = loggerFactory.CreateLogger<StorageCapacityNotification>();
        _config = new(configuration);
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _azureManagementService = azureManagementService;
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
    public async Task RunStorageCapacityNotification([QueueTrigger("storage-capacity", Connection = "DatahubStorageConnectionString")] string queueItem, 
        CancellationToken cancellationToken)
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

        // get used capacity from Azure
        var usedCapacity = await GetStorageUsedCapacity(msg.ResourceGroup, msg.StorageAccount, cancellationToken);
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
        try
        {
            var content = JsonSerializer.Deserialize<DBResourceContent>(row.JsonContent);
            if (string.IsNullOrEmpty(content?.resource_group_name) || string.IsNullOrEmpty(content?.storage_account))
                return default;

            var msgObj = new StorageAccountMessage(row.ProjectId, content.resource_group_name, content.storage_account);
            var message = JsonSerializer.Serialize(msgObj, GetJsonSerializerOptions());

            return EncodeBase64(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to deserialize resource JsonContent\n{row.JsonContent}.");
            return default;
        }
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
            var queueClient = new QueueClient(_config.StorageQueueConnection, "email-notifications");

            var msg = new EmailRequestMessage()
            {
                To = new(contacts),
                Subject = "Datahub storage capacity update / Mise à jour de la capacité de stockage Datahub",
                Body = $"Notice, {perc}% of your Datahub Storage Capacity has been reached. \nNotez que {perc} % de votre capacité de stockage Datahub a été atteinte."
            };

            var messageJson = JsonSerializer.Serialize(msg, GetJsonSerializerOptions());
            await queueClient.SendMessageAsync(EncodeBase64(messageJson));
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

    private async Task<double?> GetStorageUsedCapacity(string resourceGroup, string storageAccount, CancellationToken cancellationToken)
    {
        var azureManagementSession = await _azureManagementService.GetSession(cancellationToken);
        return await azureManagementSession.GetStorageUsedCapacity(resourceGroup, storageAccount);
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
}

record DBResourceContent(string resource_group_name, string storage_account);

record StorageAccountMessage(int ProjectId, string ResourceGroup, string StorageAccount);
