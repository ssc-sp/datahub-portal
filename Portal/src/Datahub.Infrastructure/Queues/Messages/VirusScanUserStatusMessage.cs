namespace Datahub.Infrastructure.Queues.Messages;

/// <summary>
/// Message sent to service functions to handle user status and file access permissions
/// based on virus scan results
/// </summary>
public class VirusScanUserStatusMessage
{
    /// <summary>
    /// Workspace acronym that owns the file
    /// </summary>
    public required string WorkspaceAcronym { get; init; }

    /// <summary>
    /// User's object ID who uploaded the file
    /// </summary>
    public string? UploaderObjectId { get; init; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string? UploaderEmail { get; init; }

    /// <summary>
    /// User's display name
    /// </summary>
    public string? UploaderName { get; init; }

    /// <summary>
    /// File name that was scanned
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Blob path relative to the container
    /// </summary>
    public required string BlobPath { get; init; }

    /// <summary>
    /// Scan status: "Clean", "Infected", "Failed"
    /// </summary>
    public required string ScanStatus { get; init; }

    /// <summary>
    /// Timestamp when the scan completed
    /// </summary>
    public required DateTimeOffset ScanCompletedOn { get; init; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSizeBytes { get; init; }

    /// <summary>
    /// SHA256 hash of the file
    /// </summary>
    public string? FileHashSha256 { get; init; }

    /// <summary>
    /// Storage account name
    /// </summary>
    public string? StorageAccountName { get; init; }

    /// <summary>
    /// Container name
    /// </summary>
    public string ContainerName { get; init; } = "datahub";

    /// <summary>
    /// Scanner engine used (e.g., "ClamAV")
    /// </summary>
    public string? ScanEngine { get; init; }

    /// <summary>
    /// Event correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Whether ACLs were successfully applied
    /// </summary>
    public bool AclsApplied { get; init; }

    /// <summary>
    /// Additional metadata from the scan
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
