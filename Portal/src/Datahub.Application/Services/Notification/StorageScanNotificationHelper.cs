using System;
using System.Collections.Generic;
using System.IO;
using Datahub.Application.Configuration;

namespace Datahub.Application.Services.Notification;

public static class StorageScanNotificationHelper
{
    public static string NormalizeBlobPath(string? blobPath)
    {
        return string.IsNullOrWhiteSpace(blobPath)
            ? string.Empty
            : blobPath.Replace('\\', '/').Trim('/');
    }

    public static string ResolveWorkspace(string? workspaceAcronym) =>
        string.IsNullOrWhiteSpace(workspaceAcronym)
            ? "DataHub"
            : workspaceAcronym.Trim();

    public static string ResolveFileName(string? fileName, string normalizedBlobPath)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        var candidate = Path.GetFileName(normalizedBlobPath);
        return string.IsNullOrWhiteSpace(candidate) ? normalizedBlobPath : candidate!;
    }

    public static DateTimeOffset ResolveScanCompletedOn(DateTimeOffset scanCompletedOn) =>
        scanCompletedOn == default ? DateTimeOffset.UtcNow : scanCompletedOn;

    public static StorageScanSuccessEventPayload BuildEventPayload(StorageScanSuccessNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var normalizedPath = NormalizeBlobPath(notification.BlobPath);
        var container = string.IsNullOrWhiteSpace(notification.ContainerName)
            ? "datahub"
            : notification.ContainerName.Trim('/');

        return new StorageScanSuccessEventPayload
        {
            WorkspaceAcronym = notification.WorkspaceAcronym,
            StorageAccountName = notification.StorageAccountName,
            ContainerName = container,
            BlobPath = normalizedPath,
            FileName = ResolveFileName(notification.FileName, normalizedPath),
            FileSizeBytes = notification.FileSizeBytes,
            FileHashSha256 = notification.FileHashSha256,
            ScanCompletedOn = ResolveScanCompletedOn(notification.ScanCompletedOn),
            ScanEngine = string.IsNullOrWhiteSpace(notification.ScanEngine) ? "ClamAV" : notification.ScanEngine,
            UploadedBy = notification.UploadedBy,
            UploadedByEmail = notification.UploadedByEmail,
            UploadedByObjectId = notification.UploadedByObjectId,
            CorrelationId = notification.CorrelationId,
            Metadata = notification.Metadata
        };
    }

    public static string BuildEventSubject(StorageScanNotificationSettings settings, string workspace, string container, string blobPath)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var prefix = string.IsNullOrWhiteSpace(settings.SubjectPrefix)
            ? "/datahub/storage/scan"
            : settings.SubjectPrefix.TrimEnd('/');

        var workspaceSegment = string.IsNullOrWhiteSpace(workspace)
            ? "UNKNOWN"
            : workspace.Trim().ToUpperInvariant();

        var containerSegment = string.IsNullOrWhiteSpace(container)
            ? "datahub"
            : container.Trim('/');

        var blobSegment = string.IsNullOrWhiteSpace(blobPath) ? "-" : blobPath;

        return $"{prefix}/{workspaceSegment}/{containerSegment}/{blobSegment}";
    }

    public sealed record StorageScanSuccessEventPayload
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
