using System.Text;
using Azure.Storage.Blobs;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;

namespace Datahub.Infrastructure.Services.Notification;

public class GCNotifyService : IGCNotifyService
{
    private IKeyVaultService _keyVaultService;
    private string _mappingsJson;

    public GCNotifyService(IKeyVaultService keyVaultService, DatahubPortalConfiguration portalConfiguration)
    {
        _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));

        if (portalConfiguration?.Media?.StorageConnectionString is null) throw new UnauthorizedAccessException("No token available");

        _mappingsJson = GetTemplateMappings(portalConfiguration);
    }

    public string GetTemplateMappings(DatahubPortalConfiguration portalConfiguration)
    {
        var blobClient = new BlobServiceClient(portalConfiguration.Media.StorageConnectionString)
            .GetBlobContainerClient("docs")
            .GetBlobClient("gcnotify-mappings.json");

        if (blobClient.Exists())
        {
            var response = blobClient.DownloadContent();
            return response.Value.Content.ToString();
        }
        return "{}"; // Return empty JSON if the blob does not exist
    }

    public async Task SendNotification(string postDataJson)
    {
        string endpoint = "https://api.notification.canada.ca/v2/notifications/email";
        string apikey = await _keyVaultService.GetSecret("gc-notify-api-key");

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"ApiKey-v1 {apikey}");

            var content = new StringContent(postDataJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send notification: {response.StatusCode} - {errorContent}");
            }
        }
    }

    public async Task SendAccountCreatedNotification(string email)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("user-invited", _mappingsJson)
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public async Task SendAccountDeletionNoticeNotification(string email, string daysSince, string daysUntil)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("user-delete-notice", _mappingsJson),
            personalisation = new
            {
                daysSince = daysSince,
                daysUntil = daysUntil
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public async Task SendAccountLockingNoticeNotification(string email, string daysSince, string daysUntil)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("user-lock-notice", _mappingsJson),
            personalisation = new
            {
                daysSince = daysSince,
                daysUntil = daysUntil
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public async Task SendWorkspaceCostNotification(string email, string perc, string acro)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("cost-alert", _mappingsJson),
            personalisation = new
            {
                perc = perc,
                acro = acro
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public async Task SendDataHubErrorNotification(string errorMessage, string email = "datasolutions-solutiondedonnees@ssc-spc.gc.ca")
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("error", _mappingsJson),
            personalisation = new
            {
                errorMessage = errorMessage
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendDatahubResourceDeletedNotification(string email, string resource, string resource_fr, string acro)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("resource-deleted", _mappingsJson),
            personalisation = new
            {
                resource = resource,
                resource_fr = resource_fr,
                acro = acro
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public string GetTemplateId(string templateName, string mappingsJson)
    {
        var mappings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(mappingsJson);
        if (mappings != null && mappings.TryGetValue(templateName, out var templateId))
        {
            return templateId;
        }
        throw new KeyNotFoundException($"Template '{templateName}' not found in mappings.");
    }
}
