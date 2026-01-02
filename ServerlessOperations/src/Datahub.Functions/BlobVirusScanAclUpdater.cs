using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Storage;
using Datahub.Functions.Models;
using Datahub.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class BlobVirusScanAclUpdater
{
    private static readonly HashSet<string> SupportedEventTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft.Storage.BlobPropertiesUpdated",
            "Microsoft.Storage.BlobMetadataUpdated"
        };

    private readonly ILogger<BlobVirusScanAclUpdater> _logger;
    private readonly IWorkspaceAclService _workspaceAclService;
    private readonly IBlobMetadataWriter _blobMetadataWriter;
    private readonly IGCNotifyService _gcNotifyService;

    private const string ContainerName = "datahub";
    private const string UploadPrefix = "upload/";

    public BlobVirusScanAclUpdater(
        ILogger<BlobVirusScanAclUpdater> logger,
        IWorkspaceAclService workspaceAclService,
        IBlobMetadataWriter blobMetadataWriter,
        IGCNotifyService gcNotifyService)
    {
        _logger = logger;
        _workspaceAclService = workspaceAclService;
        _blobMetadataWriter = blobMetadataWriter;
        _gcNotifyService = gcNotifyService;
    }

    [Function("BlobVirusScanAclUpdater")]
    public async Task RunAsync(
        [EventGridTrigger] EventGridEvent eventGridEvent,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        if (!SupportedEventTypes.Contains(eventGridEvent.EventType))
        {
            _logger.LogDebug("Ignoring unsupported event type {EventType}", eventGridEvent.EventType);
            return;
        }

        BlobMetadataEventData? eventData;
        try
        {
            eventData = eventGridEvent.Data.ToObjectFromJson<BlobMetadataEventData>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize Event Grid payload");
            return;
        }

        if (eventData is null)
        {
            _logger.LogWarning("Event data is null for subject {Subject}", eventGridEvent.Subject);
            return;
        }

        var metadata = eventData.Metadata;
        string? scanStatus = null;
        if (metadata is null ||
            !metadata.TryGetValue("dh:scanStatus", out scanStatus) ||
            !string.Equals(scanStatus, "Clean", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Skipping blob because dh:scanStatus is not Clean (value: {Status})",
                scanStatus ?? "<missing>");
            return;
        }

        if (!TryResolveBlobInfo(eventData.Url, eventGridEvent.Subject, out var workspaceAcronym, out var blobPath))
        {
            _logger.LogWarning(
                "Unable to resolve blob path from event subject {Subject}",
                eventGridEvent.Subject);
            return;
        }

        try
        {
            await _workspaceAclService.ApplyWorkspaceMemberAclsAsync(
                workspaceAcronym,
                blobPath,
                permissions: "r--",
                recursive: false);

            await _blobMetadataWriter.SetAccessEnabledMetadataAsync(
                workspaceAcronym,
                blobPath,
                metadata,
                cancellationToken);

            await TrySendScanSuccessEmailAsync(
                workspaceAcronym,
                blobPath,
                metadata,
                eventData.Url,
                eventGridEvent,
                cancellationToken);

            _logger.LogInformation(
                "Enabled read ACLs for workspace {Workspace} blob {BlobPath}",
                workspaceAcronym,
                blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update ACLs for workspace {Workspace} blob {BlobPath}",
                workspaceAcronym,
                blobPath);
            throw;
        }
    }

    private static bool TryResolveBlobInfo(
        string? url,
        string subject,
        [NotNullWhen(true)] out string? workspaceAcronym,
        [NotNullWhen(true)] out string? blobPath)
    {
        workspaceAcronym = null;
        blobPath = null;

        var resolvedPath = TryParsePathFromUrl(url) ?? TryParsePathFromSubject(subject);
        if (resolvedPath is null)
        {
            return false;
        }

        if (!resolvedPath.StartsWith(ContainerName + '/', StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var relativePath = resolvedPath[(ContainerName.Length + 1)..];

        if (!relativePath.StartsWith(UploadPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var suffix = relativePath[UploadPrefix.Length..];
        var separatorIndex = suffix.IndexOf('/');
        if (separatorIndex <= 0)
        {
            return false;
        }

        var workspaceSegment = suffix[..separatorIndex];
        var blobSegment = suffix[(separatorIndex + 1)..];
        if (string.IsNullOrWhiteSpace(blobSegment))
        {
            return false;
        }

        workspaceAcronym = workspaceSegment.ToUpperInvariant();
        blobPath = $"{UploadPrefix}{workspaceSegment}/{blobSegment}";
        return true;
    }

    private static string? TryParsePathFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var decoded = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        return decoded.Replace("//", "/", StringComparison.Ordinal);
    }

    private static string? TryParsePathFromSubject(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            return null;
        }

        var trimmed = subject.Trim('/');
        if (!trimmed.StartsWith("blobServices/default/containers/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var remainder = trimmed["blobServices/default/containers/".Length..];
        var parts = remainder.Split('/', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return null;
        }

        if (!parts[1].Equals("blobs", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return string.Join('/', parts[0], parts[2]);
    }

    private async Task TrySendScanSuccessEmailAsync(
        string workspaceAcronym,
        string blobPath,
        IReadOnlyDictionary<string, string>? metadata,
        string? blobUrl,
        EventGridEvent eventGridEvent,
        CancellationToken cancellationToken)
    {
        if (metadata is null)
        {
            _logger.LogDebug(
                "Skipping scan success email for workspace {Workspace} blob {BlobPath}: metadata snapshot missing",
                workspaceAcronym,
                blobPath);
            return;
        }

        try
        {
            var notification = await BuildNotificationAsync(
                workspaceAcronym,
                blobPath,
                metadata,
                blobUrl,
                eventGridEvent.Id);

            if (notification is null)
            {
                return;
            }

            var payload = StorageScanNotificationHelper.BuildEventPayload(notification);
            await _gcNotifyService.SendStorageScanSuccessEmailAsync(payload, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to dispatch scan success email for workspace {Workspace} blob {BlobPath}",
                workspaceAcronym,
                blobPath);
        }
    }

    private async Task<StorageScanSuccessNotification?> BuildNotificationAsync(
        string workspaceAcronym,
        string blobPath,
        IReadOnlyDictionary<string, string> metadata,
        string? blobUrl,
        string correlationId)
    {
        var uploaderObjectId = TryGetMetadataValue(metadata, "ownedby") ??
            TryGetMetadataValue(metadata, "createdby");

        var (uploaderEmail, uploaderName) = await ResolveUploaderDetailsAsync(
            workspaceAcronym,
            uploaderObjectId);

        if (string.IsNullOrWhiteSpace(uploaderEmail))
        {
            _logger.LogInformation(
                "Skipping scan success email for workspace {Workspace} blob {BlobPath}: uploader email unavailable",
                workspaceAcronym,
                blobPath);
            return null;
        }

        var notification = new StorageScanSuccessNotification
        {
            WorkspaceAcronym = workspaceAcronym,
            StorageAccountName = ResolveStorageAccountName(blobUrl),
            ContainerName = ContainerName,
            BlobPath = blobPath,
            FileName = TryGetMetadataValue(metadata, "filename"),
            FileSizeBytes = TryParseLongMetadata(metadata, "filesize"),
            FileHashSha256 = TryGetMetadataValue(metadata, "filehash")
                ?? TryGetMetadataValue(metadata, "filehashsha256")
                ?? TryGetMetadataValue(metadata, "dh:filehash")
                ?? TryGetMetadataValue(metadata, "dh:filehashsha256"),
            ScanCompletedOn = ResolveScanCompletedOn(metadata),
            ScanEngine = TryGetMetadataValue(metadata, "dh:scanner"),
            UploadedBy = uploaderName,
            UploadedByEmail = uploaderEmail,
            UploadedByObjectId = uploaderObjectId,
            CorrelationId = correlationId,
            Metadata = metadata
        };

        return notification;
    }

    private async Task<(string? Email, string? DisplayName)> ResolveUploaderDetailsAsync(
        string workspaceAcronym,
        string? uploaderObjectId)
    {
        if (string.IsNullOrWhiteSpace(uploaderObjectId))
        {
            return default;
        }

        try
        {
            var workspace = await _workspaceAclService.GetWorkspaceAsync(workspaceAcronym);
            var portalUser = workspace?.UserRoles?
                .Select(ur => ur.PortalUser)
                .FirstOrDefault(u => u != null &&
                    string.Equals(u.GraphGuid, uploaderObjectId, StringComparison.OrdinalIgnoreCase));

            if (portalUser is null)
            {
                _logger.LogInformation(
                    "Uploader with object id {UploaderId} not found in workspace {Workspace}",
                    uploaderObjectId,
                    workspaceAcronym);
                return default;
            }

            return (portalUser.Email, portalUser.DisplayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to resolve uploader information for workspace {Workspace}",
                workspaceAcronym);
            return default;
        }
    }

    private static DateTimeOffset ResolveScanCompletedOn(IReadOnlyDictionary<string, string> metadata)
    {
        var raw = TryGetMetadataValue(metadata, "dh:scanDate");
        return !string.IsNullOrWhiteSpace(raw) && DateTimeOffset.TryParse(raw, out var timestamp)
            ? timestamp
            : DateTimeOffset.UtcNow;
    }

    private static long? TryParseLongMetadata(IReadOnlyDictionary<string, string> metadata, string key)
    {
        var value = TryGetMetadataValue(metadata, key);
        if (!string.IsNullOrWhiteSpace(value) && long.TryParse(value, out var result))
        {
            return result;
        }

        return null;
    }

    private static string? TryGetMetadataValue(IReadOnlyDictionary<string, string> metadata, string key)
    {
        foreach (var pair in metadata)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }

    private static string? ResolveStorageAccountName(string? blobUrl)
    {
        if (string.IsNullOrWhiteSpace(blobUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var host = uri.Host;
        var segments = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length > 0 ? segments[0] : null;
    }
}
