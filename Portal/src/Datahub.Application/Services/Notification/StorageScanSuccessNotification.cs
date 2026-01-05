using System;
using System.Collections.Generic;

namespace Datahub.Application.Services.Notification;

/// <summary>
/// Represents the payload that will be emitted when ClamAV (or any AV engine)
/// reports that a workspace blob has been scanned successfully.
/// </summary>
public record StorageScanSuccessNotification
{
    /// <summary>
    /// Workspace acronym that owns the file. Required.
    /// </summary>
    public required string WorkspaceAcronym { get; init; }

    /// <summary>
    /// Name of the storage account hosting the workspace container.
    /// </summary>
    public string? StorageAccountName { get; init; }

    /// <summary>
    /// Container that stores the blob. Defaults to <c>datahub</c>.
    /// </summary>
    public string ContainerName { get; init; } = "datahub";

    /// <summary>
    /// Blob path relative to the container (e.g. upload/ABC123/file.csv). Required.
    /// </summary>
    public required string BlobPath { get; init; }

    /// <summary>
    /// Optional friendly file name to surface to users; falls back to <see cref="BlobPath"/>.
    /// </summary>
    public string? FileName { get; init; }

    /// <summary>
    /// File size in bytes, if known.
    /// </summary>
    public long? FileSizeBytes { get; init; }

    /// <summary>
    /// Hash of the file contents (SHA256) to help downstream dedupe or auditing.
    /// </summary>
    public string? FileHashSha256 { get; init; }

    /// <summary>
    /// Time (UTC) when scanning reported success.
    /// </summary>
    public DateTimeOffset ScanCompletedOn { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Engine or service that performed the scan (e.g. ClamAV).
    /// </summary>
    public string? ScanEngine { get; init; }

    /// <summary>
    /// Identifier (AAD object id) of the user that uploaded the file, when available.
    /// </summary>
    public string? UploadedByObjectId { get; init; }

    /// <summary>
    /// Display name of the user that uploaded the file.
    /// </summary>
    public string? UploadedBy { get; init; }

    /// <summary>
    /// Email of the user that uploaded the file.
    /// </summary>
    public string? UploadedByEmail { get; init; }

    /// <summary>
    /// Optional correlation id tying the notification to the ClamAV job/execution.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Additional metadata captured during scanning (e.g. scan duration, version, etc.).
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
