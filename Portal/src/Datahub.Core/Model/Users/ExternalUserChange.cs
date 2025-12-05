using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Users;

/// <summary>
/// Immutable audit row describing a single change on an ExternalUser.
/// </summary>
public class ExternalUserChange
{
 /// <summary>
 /// Gets or sets the surrogate key of the change entry.
 /// </summary>
 public int Id { get; set; }

 /// <summary>
 /// Gets or sets the foreign key to the ExternalUser that was changed.
 /// </summary>
 public int ExternalUserId { get; set; }

 /// <summary>
 /// Gets or sets the navigation to the ExternalUser record that was changed.
 /// </summary>
 public ExternalUser ExternalUser { get; set; } = null!;

 /// <summary>
 /// Gets or sets the optional foreign key to the workspace (project) where the change occurred or which initiated it.
 /// </summary>
 public int? ProjectId { get; set; }

 /// <summary>
 /// Gets or sets the navigation to the related workspace (project).
 /// </summary>
 public Datahub_Project? Project { get; set; }

 /// <summary>
 /// Gets or sets the field name that changed (e.g., OID, Email, Status).
 /// </summary>
 public string Field { get; set; } = string.Empty;

 /// <summary>
 /// Gets or sets the previous value (short string snapshot).
 /// </summary>
 public string? OldValue { get; set; }

 /// <summary>
 /// Gets or sets the new value (short string snapshot).
 /// </summary>
 public string? NewValue { get; set; }

 /// <summary>
 /// Gets or sets the change type (e.g., New, Edit, Delete, Deactivate, Reactivate, InviteAccepted).
 /// </summary>
 public string ChangeType { get; set; } = string.Empty;

 /// <summary>
 /// Gets or sets the optional foreign key to the PortalUser who performed the change.
 /// </summary>
 public int? ChangedById { get; set; }

 /// <summary>
 /// Gets or sets the actor navigation (the PortalUser who performed the change).
 /// </summary>
 public PortalUser? ChangedBy { get; set; }

 /// <summary>
 /// Gets or sets the UTC timestamp when the change happened.
 /// </summary>
 public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

 /// <summary>
 /// Gets or sets an optional free-form reason or context for the change.
 /// </summary>
 public string? Reason { get; set; }
}
