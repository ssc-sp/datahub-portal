using System;
using System.ComponentModel.DataAnnotations;
using Datahub.Core.Model.Achievements;
using Elemental.Components;

namespace Datahub.Core.Model.Projects;

public class Datahub_Project_User {

    [AeFormIgnore]
    [Key]

    public int ProjectUser_ID { get; set; }

    [Obsolete("Use PortalUser reference instead")]
    [StringLength(200)]
    public string User_ID { get; set; }
    
    public int PortalUserId { get; set; }
    public DateTime? Approved_DT { get; set; }

    [Obsolete("Use ApprovedPortalUser reference instead")]
    public string ApprovedUser { get; set; }
    public int ApprovedPortalUserId { get; set; }
    
    public int RoleId { get; set; }

    public Project_Role Role { get; set; }
    public PortalUser PortalUser { get; set; }
    public PortalUser ApprovedPortalUser { get; set; }
    
    public Datahub_Project Project { get; set; }

    [Obsolete("Use PortalUser reference instead")]
    [StringLength(200)]
    public string User_Name {  get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }
}