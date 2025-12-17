using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Datahub.Application.Services.Storage;
using Datahub.Functions.Services;
using Datahub.Functions.Models;
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

    private const string ContainerName = "datahub";
    private const string UploadPrefix = "upload/";

    public BlobVirusScanAclUpdater(
        ILogger<BlobVirusScanAclUpdater> logger,
        IWorkspaceAclService workspaceAclService,
        IBlobMetadataWriter blobMetadataWriter)
    {
        _logger = logger;
        _workspaceAclService = workspaceAclService;
        _blobMetadataWriter = blobMetadataWriter;
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
}
