using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Achievements;
using Elemental.Components;

namespace Datahub.Core.Model.Projects;

public class Datahub_Project_User
{
    [AeFormIgnore]
    [Key]
    public int ProjectUser_ID { get; set; }
    public int? PortalUserId { get; set; }
    public int? ApprovedPortalUserId { get; set; }
    public int? RoleId { get; set; }
    public int Project_ID { get; set; }
    public DateTime? Approved_DT { get; set; }
    public bool IsDataSteward { get; set; }

    #region Navigation Properties
    public Datahub_Project Project { get; set; }
    public Project_Role Role { get; set; }
    public PortalUser PortalUser { get; set; }
    public PortalUser ApprovedPortalUser { get; set; }
    #endregion

    #region Deprecated
    [Obsolete("Use PortalUser reference instead")]
    [StringLength(200)]
    public string User_Name { get; set; }

    [Obsolete("Use Role reference instead")]
    public bool IsDataApprover { get; set; }
    [Obsolete("Use Role reference instead")]
    public bool IsAdmin { get; set; }

    [Obsolete("Use PortalUser reference instead")]
    [StringLength(200)]
    public string User_ID { get; set; }

    [Obsolete("Use ApprovedPortalUser reference instead")]
    public string ApprovedUser { get; set; }
    #endregion

    [Timestamp]
    public byte[] Timestamp { get; set; }
}