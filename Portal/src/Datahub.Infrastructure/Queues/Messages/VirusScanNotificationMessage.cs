namespace Datahub.Infrastructure.Queues.Messages;

/// <summary>
/// Message sent to create system notifications for users about virus scan completion.
/// Uses the existing SystemNotificationService (database-backed, polling-based).
/// </summary>
public class VirusScanNotificationMessage
{
    /// <summary>
    /// Workspace acronym that owns the file
    /// </summary>
    public required string WorkspaceAcronym { get; init; }

    /// <summary>
    /// User's object ID (for targeted notifications)
    /// </summary>
    public string? UserObjectId { get; init; }

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
    /// Storage account name
    /// </summary>
    public string? StorageAccountName { get; init; }

    /// <summary>
    /// Container name
    /// </summary>
    public string ContainerName { get; init; } = "datahub";

    /// <summary>
    /// Event correlation ID for tracking
    /// </summary>
    public string? CorrelationId { get; init; }
}
