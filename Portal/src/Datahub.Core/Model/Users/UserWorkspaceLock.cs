using Datahub.Shared.Entities;

namespace Datahub.Core.Model.Users;

/// <summary>
/// Represents an audit record for global user lock/unlock events.
/// Tracks the complete history of locks, unlocks, and evidence uploads.
/// </summary>
public class UserWorkspaceLock
{
    /// <summary>
    /// Gets or sets the unique identifier for this lock event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the portal user ID associated with this lock event.
    /// </summary>
    public int PortalUserId { get; set; }

    /// <summary>
    /// Gets or sets the portal user associated with this lock event.
    /// </summary>
    public PortalUser? User { get; set; }

    /// <summary>
    /// Gets or sets the type of event: "Locked", "Unlocked", "EvidenceUploaded"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when this event occurred.
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Gets or sets the reason for this event (why locked/unlocked).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the URL to the evidence file (virus scan report, etc.).
    /// Only populated for lock events or evidence uploads.
    /// </summary>
    public string? EvidenceUrl { get; set; }

    /// <summary>
    /// Gets or sets the portal user ID of the admin who performed this action.
    /// </summary>
    public int? PerformedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the admin user who performed this action.
    /// </summary>
    public PortalUser? PerformedByUser { get; set; }

    /// <summary>
    /// Gets or sets additional notes about this event.
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Event types for user workspace locks
/// </summary>
public static class LockEventType
{
    public const string Locked = "Locked";
    public const string Unlocked = "Unlocked";
    public const string EvidenceUploaded = "EvidenceUploaded";
}
