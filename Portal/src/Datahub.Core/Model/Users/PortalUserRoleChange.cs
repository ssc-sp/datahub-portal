using Datahub.Core.Model.Datahub;
using static Datahub.Core.Model.Projects.Project_Role;

namespace Datahub.Core.Model.Users;

/// <summary>
/// Represents a record of status changes for a portal user.
/// </summary>
public class PortalUserRoleChange
{
    /// <summary>
    /// Gets or sets the unique identifier of the status change record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the portal user whose status changed.
    /// </summary>
    public int PortalUserId { get; set; }

    /// <summary>
    /// Gets or sets the role associated with this change.
    /// </summary>
    public RoleNames RoleId { get; set; }

    /// <summary>
    /// Gets or sets the date and time the user's status changed.
    /// </summary>
    public DateTime ChangeDate { get; set; }
}
