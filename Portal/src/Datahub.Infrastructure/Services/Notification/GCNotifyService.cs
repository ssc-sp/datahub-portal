using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Datahub.Infrastructure.Services.Notification;


public class GCNotifyService : IGCNotifyService
{
    private const string SuccessfulScanTemplateName = "successful-scan";
    private readonly IKeyVaultService _keyVaultService;
    private readonly ILogger<GCNotifyService> _logger;
    private readonly DatahubPortalConfiguration _portalConfiguration;
    private readonly StorageScanNotificationSettings _scanNotificationSettings;
    private readonly SemaphoreSlim _scanClientLock = new(1, 1);
    private readonly string _mappingsJson;
    private EventGridPublisherClient? _scanEventGridClient;

    public GCNotifyService(
        IKeyVaultService keyVaultService,
        ILoggerFactory loggerFactory,
        DatahubPortalConfiguration portalConfiguration)
    {
        _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        _logger = loggerFactory.CreateLogger<GCNotifyService>();
        _portalConfiguration = portalConfiguration ?? throw new ArgumentNullException(nameof(portalConfiguration));
        _scanNotificationSettings = _portalConfiguration.StorageScanNotifications ?? new();

        if (_portalConfiguration.Media?.StorageConnectionString is null)
        {
            _logger.LogError("Initialization failed: Media.StorageConnectionString is null (no token available).");
            throw new UnauthorizedAccessException("No token available");
        }

        _logger.LogInformation("Initializing GCNotifyService and loading template mappings from blob storage.");
        _mappingsJson = GetTemplateMappings(_portalConfiguration);
        _logger.LogInformation("GCNotifyService initialized successfully. Templates loaded: {TemplateCount}", SafeCountMappings(_mappingsJson));
    }

    public string GetTemplateMappings(DatahubPortalConfiguration portalConfiguration)
    {
        _logger.LogDebug("Retrieving GC Notify template mappings from blob storage.");
        var blobClient = new BlobServiceClient(portalConfiguration.Media.StorageConnectionString)
            .GetBlobContainerClient("docs")
            .GetBlobClient("gcnotify-mappings.json");

        try
        {
            if (blobClient.Exists())
            {
                var response = blobClient.DownloadContent();
                var json = response.Value.Content.ToString();
                _logger.LogInformation("Template mappings blob found. Length={Length} chars.", json.Length);
                return json;
            }

            _logger.LogWarning("Template mappings blob not found. Falling back to empty mappings.");
            return "{}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve template mappings. Returning empty mappings.");
            return "{}";
        }
    }

    public async Task SendNotification(string postDataJson)
    {
        _logger.LogDebug("Preparing to send GC Notify request. Payload length={Length}", postDataJson?.Length);

        const string endpoint = "https://api.notification.canada.ca/v2/notifications/email";

        try
        {
            var apikey = await _keyVaultService.GetSecret("gc-notify-api-key");
            if (string.IsNullOrWhiteSpace(apikey))
            {
                _logger.LogError("GC Notify API key is null or empty.");
                throw new InvalidOperationException("API key not available.");
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"ApiKey-v1 {apikey}");

            using var content = new StringContent(postDataJson, Encoding.UTF8, "application/json");
            _logger.LogInformation("Sending GC Notify request to {Endpoint}", endpoint);

            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("GC Notify request failed. Status={StatusCode}, Body={Error}", response.StatusCode, Truncate(errorContent, 2000));
                throw new Exception($"Failed to send notification: {response.StatusCode} - {errorContent}");
            }

            _logger.LogInformation("GC Notify request succeeded. Status={StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while sending GC Notify notification.");
            throw;
        }
    }

    public async Task SendAccountCreatedNotification(string email)
    {
        using var _ = _logger.BeginScope("AccountCreatedNotification {Email}", MaskEmail(email));
        _logger.LogInformation("Composing account created notification.");

        var templateId = GetTemplateId("user-invited", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId
        };

        _logger.LogDebug("Dispatching account created notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendAccountDeletionNoticeNotification(string email, string daysSince, string daysUntil)
    {
        using var _ = _logger.BeginScope("AccountDeletionNotice {Email}", MaskEmail(email));
        _logger.LogInformation("Composing account deletion notice. daysSince={DaysSince}, daysUntil={DaysUntil}", daysSince, daysUntil);

        var templateId = GetTemplateId("user-delete-notice", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { daysSince, daysUntil }
        };

        _logger.LogDebug("Dispatching account deletion notice with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendAccountLockingNoticeNotification(string email, string daysSince, string daysUntil)
    {
        using var _ = _logger.BeginScope("AccountLockingNotice {Email}", MaskEmail(email));
        _logger.LogInformation("Composing account locking notice. daysSince={DaysSince}, daysUntil={DaysUntil}", daysSince, daysUntil);

        var templateId = GetTemplateId("user-lock-notice", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { daysSince, daysUntil }
        };

        _logger.LogDebug("Dispatching account locking notice with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendWorkspaceCostNotification(string email, string perc, string acro)
    {
        using var _ = _logger.BeginScope("WorkspaceCostNotification {Email}", "<redacted>");
        _logger.LogInformation("Composing workspace cost notification. perc={Perc}, acro={Acro}", perc, acro);

        var templateId = GetTemplateId("cost-alert", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { perc, acro }
        };

        _logger.LogDebug("Dispatching workspace cost notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendWorkspaceInactiveNotification(string email, string daysSinceLastLogin)
    {
        using var _ = _logger.BeginScope("WorkspaceInactiveNotification {Email}", "<redacted>");
        _logger.LogInformation("Composing workspace inactive notification. daysSinceLastLogin={daysSinceLastLogin}", daysSinceLastLogin);
        var templateId = GetTemplateId("workspace-inactive", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { daysSinceLastLogin }
        };
        _logger.LogDebug("Dispatching workspace inactive notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendDataHubErrorNotification(string errorMessage, string email = "datasolutions-solutiondedonnees@ssc-spc.gc.ca")
    {
        using var _ = _logger.BeginScope("DataHubErrorNotification {Email}", MaskEmail(email));
        _logger.LogWarning("Composing DataHub error notification. ErrorMessageHash={Hash}", errorMessage?.GetHashCode());

        var templateId = GetTemplateId("error", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { errorMessage }
        };

        _logger.LogDebug("Dispatching error notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendDatahubResourceDeletedNotification(string email, string resource, string resource_fr, string acro)
    {
        using var _ = _logger.BeginScope("ResourceDeletedNotification {Email}", MaskEmail(email));
        _logger.LogInformation("Composing resource deleted notification. resource={Resource} acro={Acro}", resource, acro);

        var templateId = GetTemplateId("resource-deleted", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { resource, resource_fr, acro }
        };

        _logger.LogDebug("Dispatching resource deleted notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendWelcomePackageNotification(string email)
    {
        using var _ = _logger.BeginScope("WelcomePackageNotification");
        _logger.LogInformation("Composing welcome package notification.");

        var templateId = GetTemplateId("welcome-package", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId
        };

        _logger.LogDebug("Dispatching welcome package notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }
    public async Task SendBugReportNotification(string id, string title, string body, string email = "datasolutions-solutiondedonnees@ssc-spc.gc.ca")
    {
        using var _ = _logger.BeginScope("BugReportNotification ID={Id}", id);
        _logger.LogInformation("Composing bug report notification. title={Title}", title);
        var templateId = GetTemplateId("bug-report", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { id, title, body }
        };

        _logger.LogDebug("Dispatching bug report notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendInfectedFileNotification(string email, string fileName, string workspace, string date)
    {
        using var _ = _logger.BeginScope("InfectedFileNotification {Email}", "<redacted>");
        _logger.LogInformation("Composing infected file notification. fileName={FileName}, workspace={Workspace}, date={Date}", fileName, workspace, date);
        var templateId = GetTemplateId("virus-upload-detected", _mappingsJson);
        var postData = new
        {
            email_address = email,
            template_id = templateId,
            personalisation = new { fileName, workspace, date }
        };
        _logger.LogDebug("Dispatching infected file notification with template {TemplateId}", templateId);
        string postDataJson = JsonSerializer.Serialize(postData);
        await SendNotification(postDataJson);
    }

    public async Task SendStorageScanSuccessNotificationAsync(
        StorageScanSuccessNotification notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (!_scanNotificationSettings.Enabled)
        {
            _logger.LogDebug(
                "Storage scan notifications disabled; skipping event for workspace {Workspace}",
                notification.WorkspaceAcronym);
            return;
        }

        var client = await EnsureScanEventGridClientAsync(cancellationToken).ConfigureAwait(false);
        if (client is null)
        {
            _logger.LogWarning(
                "Unable to publish storage scan notification for workspace {Workspace}: Event Grid client not configured",
                notification.WorkspaceAcronym);
            return;
        }

        var container = string.IsNullOrWhiteSpace(notification.ContainerName)
            ? "datahub"
            : notification.ContainerName.Trim('/');
        var normalizedPath = NormalizeBlobPath(notification.BlobPath);
        var fileName = string.IsNullOrWhiteSpace(notification.FileName)
            ? Path.GetFileName(normalizedPath)
            : notification.FileName;

        var eventData = new StorageScanSuccessEventData
        {
            WorkspaceAcronym = notification.WorkspaceAcronym,
            StorageAccountName = notification.StorageAccountName,
            ContainerName = container,
            BlobPath = normalizedPath,
            FileName = string.IsNullOrWhiteSpace(fileName) ? normalizedPath : fileName!,
            FileSizeBytes = notification.FileSizeBytes,
            FileHashSha256 = notification.FileHashSha256,
            ScanCompletedOn = notification.ScanCompletedOn == default
                ? DateTimeOffset.UtcNow
                : notification.ScanCompletedOn,
            ScanEngine = string.IsNullOrWhiteSpace(notification.ScanEngine)
                ? "ClamAV"
                : notification.ScanEngine,
            UploadedBy = notification.UploadedBy,
            UploadedByEmail = notification.UploadedByEmail,
            UploadedByObjectId = notification.UploadedByObjectId,
            CorrelationId = notification.CorrelationId,
            Metadata = notification.Metadata
        };

        var subject = BuildScanEventSubject(
            notification.WorkspaceAcronym,
            container,
            eventData.BlobPath);

        var eventType = string.IsNullOrWhiteSpace(_scanNotificationSettings.EventType)
            ? "Datahub.Storage.ScanCompleted"
            : _scanNotificationSettings.EventType;
        var dataVersion = string.IsNullOrWhiteSpace(_scanNotificationSettings.DataVersion)
            ? "1.0"
            : _scanNotificationSettings.DataVersion;

        var egEvent = new EventGridEvent(
            subject,
            eventType,
            dataVersion,
            BinaryData.FromObjectAsJson(eventData))
        {
            EventTime = eventData.ScanCompletedOn
        };

        await client.SendEventAsync(egEvent, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Published storage scan success notification for workspace {Workspace} blob {BlobPath}",
            notification.WorkspaceAcronym,
            eventData.BlobPath);
    }

    public async Task SendStorageScanSuccessEmailAsync(
        StorageScanSuccessNotification notification,
        string? recipientEmail = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();

        var targetEmail = string.IsNullOrWhiteSpace(recipientEmail)
            ? notification.UploadedByEmail
            : recipientEmail;

        if (string.IsNullOrWhiteSpace(targetEmail))
        {
            _logger.LogWarning(
                "Skipping scan success email for workspace {Workspace}: recipient email missing",
                notification.WorkspaceAcronym);
            return;
        }

        var normalizedPath = NormalizeBlobPath(notification.BlobPath);
        var resolvedFileName = string.IsNullOrWhiteSpace(notification.FileName)
            ? Path.GetFileName(normalizedPath)
            : notification.FileName!;

        if (string.IsNullOrWhiteSpace(resolvedFileName))
        {
            resolvedFileName = normalizedPath;
        }

        var scanCompletedOn = notification.ScanCompletedOn == default
            ? DateTimeOffset.UtcNow
            : notification.ScanCompletedOn;

        var templateId = GetTemplateId(SuccessfulScanTemplateName, _mappingsJson);

        var postData = new
        {
            email_address = targetEmail,
            template_id = templateId,
            personalisation = new
            {
                filename = string.IsNullOrWhiteSpace(resolvedFileName) ? normalizedPath : resolvedFileName,
                ws = string.IsNullOrWhiteSpace(notification.WorkspaceAcronym) ? "DataHub" : notification.WorkspaceAcronym,
                date = scanCompletedOn.ToString("yyyy-MM-dd HH:mm 'UTC'")
            }
        };

        await SendNotification(JsonSerializer.Serialize(postData)).ConfigureAwait(false);

        _logger.LogInformation(
            "Sent storage scan success email for workspace {Workspace} via GC Notify template",
            notification.WorkspaceAcronym);
    }

    public string GetTemplateId(string templateName, string mappingsJson)
    {
        _logger.LogDebug("Resolving template id for templateName={TemplateName}", templateName);

        try
        {
            var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(mappingsJson);
            if (mappings != null && mappings.TryGetValue(templateName, out var templateId))
            {
                _logger.LogInformation("Template resolved. templateName={TemplateName} templateId={TemplateId}", templateName, templateId);
                return templateId;
            }

            _logger.LogError("Template not found in mappings. templateName={TemplateName}", templateName);
            throw new KeyNotFoundException($"Template '{templateName}' not found in mappings.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize template mappings JSON.");
            throw;
        }
    }

    public async Task<bool> CheckHealthAsync()
    {
        _logger.LogInformation("Performing GC Notify health check using simulate-delivered address.");
        try
        {
            //https://documentation.notification.canada.ca/en/testing.html#smoke-testing
            // This will send a notification to one of the test emails
            await SendAccountCreatedNotification("simulate-delivered@notification.canada.ca");
            _logger.LogInformation("Health check succeeded.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GC Notify health check failed.");
            return false;
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "<null>";
        var at = email.IndexOf('@');
        if (at <= 1) return "***" + email[(at >= 0 ? at : email.Length)..];
        return email[0] + "***" + email[(at - 1)..];
    }

    private static int SafeCountMappings(string json)
    {
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string Truncate(string value, int max) =>
        string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value.Substring(0, max) + "...(truncated)");

    private async Task<EventGridPublisherClient?> EnsureScanEventGridClientAsync(CancellationToken cancellationToken)
    {
        if (!_scanNotificationSettings.Enabled)
        {
            return null;
        }

        if (_scanEventGridClient is not null)
        {
            return _scanEventGridClient;
        }

        await _scanClientLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_scanEventGridClient is not null)
            {
                return _scanEventGridClient;
            }

            if (string.IsNullOrWhiteSpace(_scanNotificationSettings.TopicEndpoint))
            {
                _logger.LogWarning("Scan notification topic endpoint is not configured.");
                return null;
            }

            if (!Uri.TryCreate(_scanNotificationSettings.TopicEndpoint, UriKind.Absolute, out var endpoint))
            {
                _logger.LogError("Invalid scan notification topic endpoint: {Endpoint}", _scanNotificationSettings.TopicEndpoint);
                return null;
            }

            var key = await ResolveScanTopicKeyAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Scan notification topic key is not configured.");
                return null;
            }

            _scanEventGridClient = new EventGridPublisherClient(endpoint, new AzureKeyCredential(key));
            return _scanEventGridClient;
        }
        finally
        {
            _scanClientLock.Release();
        }
    }

    private async Task<string?> ResolveScanTopicKeyAsync()
    {
        if (!string.IsNullOrWhiteSpace(_scanNotificationSettings.TopicKey))
        {
            return _scanNotificationSettings.TopicKey;
        }

        if (string.IsNullOrWhiteSpace(_scanNotificationSettings.TopicKeySecretName))
        {
            return null;
        }

        try
        {
            return await _keyVaultService.GetSecret(_scanNotificationSettings.TopicKeySecretName).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve Event Grid topic key secret {Secret}",
                _scanNotificationSettings.TopicKeySecretName);
            return null;
        }
    }

    private string BuildScanEventSubject(string workspace, string container, string blobPath)
    {
        var prefix = string.IsNullOrWhiteSpace(_scanNotificationSettings.SubjectPrefix)
            ? "/datahub/storage/scan"
            : _scanNotificationSettings.SubjectPrefix;
        var workspaceSegment = string.IsNullOrWhiteSpace(workspace)
            ? "UNKNOWN"
            : workspace.Trim().ToUpperInvariant();
        var containerSegment = string.IsNullOrWhiteSpace(container) ? "datahub" : container;
        var blobSegment = string.IsNullOrWhiteSpace(blobPath) ? "-" : blobPath;

        var baseSubject = prefix.TrimEnd('/');
        return $"{baseSubject}/{workspaceSegment}/{containerSegment}/{blobSegment}";
    }

    private static string NormalizeBlobPath(string blobPath)
    {
        return string.IsNullOrWhiteSpace(blobPath)
            ? string.Empty
            : blobPath.Replace('\\', '/').Trim('/');
    }

    private sealed record StorageScanSuccessEventData
    {
        public required string WorkspaceAcronym { get; init; }
        public string? StorageAccountName { get; init; }
        public required string ContainerName { get; init; }
        public required string BlobPath { get; init; }
        public required string FileName { get; init; }
        public long? FileSizeBytes { get; init; }
        public string? FileHashSha256 { get; init; }
        public DateTimeOffset ScanCompletedOn { get; init; }
        public string? ScanEngine { get; init; }
        public string? UploadedBy { get; init; }
        public string? UploadedByEmail { get; init; }
        public string? UploadedByObjectId { get; init; }
        public string? CorrelationId { get; init; }
        public IReadOnlyDictionary<string, string>? Metadata { get; init; }
    }
}