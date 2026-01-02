using System;
using System.Collections.Generic;

namespace Datahub.Application.Services.Notification;

/// <summary>
/// Represents the payload emitted when a workspace blob finishes an antivirus scan successfully.
/// </summary>
public record StorageScanSuccessNotification
{
    public required string WorkspaceAcronym { get; init; }
    public string? StorageAccountName { get; init; }
    public string ContainerName { get; init; } = "datahub";
    public required string BlobPath { get; init; }
    public string? FileName { get; init; }
    public long? FileSizeBytes { get; init; }
    public string? FileHashSha256 { get; init; }
    public DateTimeOffset ScanCompletedOn { get; init; } = DateTimeOffset.UtcNow;
    public string? ScanEngine { get; init; }
    public string? UploadedBy { get; init; }
    public string? UploadedByEmail { get; init; }
    public string? UploadedByObjectId { get; init; }
    public string? CorrelationId { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
