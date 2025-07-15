using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Datahub.Infrastructure.Services.Notification;

public class GCNotifyService : IGCNotifyService
{
    private IKeyVaultService _keyVaultService;
    private string _mappingsJson;

    public GCNotifyService(IKeyVaultService keyVaultService, DatahubPortalConfiguration portalConfiguration)
    {
        _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));

        if (portalConfiguration?.Media?.StorageConnectionString is null) throw new UnauthorizedAccessException("No token available");

        var blobClient = new BlobServiceClient(portalConfiguration.Media.StorageConnectionString)
            .GetBlobContainerClient("docs")
            .GetBlobClient("gcnotify-mappings.json");

        if (blobClient.Exists())
        {
            var response = blobClient.DownloadContent();
            _mappingsJson = response.Value.Content.ToString();
        }
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
            template_id = GetTemplateId("user-invited")
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public async Task SendAccountDeletionNoticeNotification(string email, string daysSince, string daysUntil)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("user-delete-notice"),
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
            template_id = GetTemplateId("user-lock-notice"),
            personalisation = new
            {
                daysSince = daysSince,
                daysUntil = daysUntil
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    public async Task SendWorkspaceCostNotification(string email, string perc)
    {
        var postData = new
        {
            email_address = email,
            template_id = GetTemplateId("cost-alert"),
            personalisation = new
            {
                perc = perc
            }
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

        await SendNotification(postDataJson);
    }

    private string GetTemplateId(string templateName)
    {
        var mappings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(_mappingsJson);
        if (mappings != null && mappings.TryGetValue(templateName, out var templateId))
        {
            return templateId;
        }
        throw new KeyNotFoundException($"Template '{templateName}' not found in mappings.");
    }
}
