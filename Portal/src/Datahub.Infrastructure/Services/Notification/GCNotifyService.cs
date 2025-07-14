using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;

namespace Datahub.Infrastructure.Services.Notification;

public class GCNotifyService : IGCNotifyService
{
    private readonly IKeyVaultService _keyVaultService;

    public GCNotifyService(IKeyVaultService keyVaultService)
    {
        _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
    }

    public async Task SendNotification(string email, string templateId)
    {
        string endpoint = "https://api.notification.canada.ca/v2/notifications/email";
        string apikey = await _keyVaultService.GetSecret("gc-notify-api-key");

        var postData = new
        {
            email_address = email,
            template_id = templateId
        };

        string postDataJson = System.Text.Json.JsonSerializer.Serialize(postData);

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
}
