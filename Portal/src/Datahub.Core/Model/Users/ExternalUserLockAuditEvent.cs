namespace Datahub.Core.Model.Users;

/// <summary>
/// Represents an audit record for global external user lock/unlock lifecycle events.
/// Tracks the complete history of lock state changes and evidence uploads.
/// </summary>
public class ExternalUserLockAuditEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for this event.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the portal user ID associated with this event.
    /// </summary>
    public int PortalUserId { get; set; }

    /// <summary>
    /// Gets or sets the portal user associated with this event.
    /// </summary>
    public PortalUser? User { get; set; }

    /// <summary>
    /// Gets or sets the event type: "Locked", "Unlocked", "EvidenceUploaded".
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when this event occurred.
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Gets or sets the reason for this event.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the URL to the evidence file.
    /// </summary>
    public string? EvidenceUrl { get; set; }

    /// <summary>
    /// Gets or sets the external user expiry value before this event was applied.
    /// </summary>
    public DateTimeOffset? PreviousExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the external user expiry value applied by this event.
    /// </summary>
    public DateTimeOffset? AppliedExpiryDate { get; set; }

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
/// Event types for external user lock audit events.
/// </summary>
public static class ExternalUserLockEventType
{
    public const string Locked = "Locked";
    public const string Unlocked = "Unlocked";
    public const string EvidenceUploaded = "EvidenceUploaded";
}
