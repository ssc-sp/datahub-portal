using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.UserTracking;

namespace Datahub.Core.Model.Achievements;

public class PortalUserStatusChange
{
    public int Id { get; set; }
    public int PortalUserId { get; set; }
    public PortalUserStatus StatusId { get; set; } // Enum-based status
    public DateTime ChangeDate { get; set; }
}

public enum PortalUserStatus
{
    Active = 1,
    Disabled = 2,
    Pending = 3,
    Suspended = 4
}
