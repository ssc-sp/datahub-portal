using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Achievements;
using Elemental.Components;

namespace Datahub.Core.Model.Projects;

public class DatahubProjectUser
{
    [AeFormIgnore]
    [Key]
    public int ProjectUserID { get; set; }
    public int? PortalUserId { get; set; }
    public int? ApprovedPortalUserId { get; set; }
    public int? RoleId { get; set; }
    public int ProjectID { get; set; }
    public DateTime? ApprovedDT { get; set; }

    public DatahubProject Project { get; set; }
    public ProjectRole Role { get; set; }
    public PortalUser PortalUser { get; set; }
    public PortalUser ApprovedPortalUser { get; set; }

    [Obsolete("Use PortalUser reference instead")]
    [StringLength(200)]
    public string UserName { get; set; }

    [Obsolete("Use Role reference instead")]
    public bool IsDataApprover { get; set; }
    [Obsolete("Use Role reference instead")]
    public bool IsAdmin { get; set; }

    [Obsolete("Use PortalUser reference instead")]
    [StringLength(200)]
    public string UserID { get; set; }

    [Obsolete("Use ApprovedPortalUser reference instead")]
    public string ApprovedUser { get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }
}