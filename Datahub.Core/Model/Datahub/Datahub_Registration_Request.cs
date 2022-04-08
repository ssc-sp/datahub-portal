using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore;

public class Datahub_Registration_Request
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(254)]
    public string Email { get; set; }
    
    [StringLength(36)]
    public Guid? LinkId { get; set; }
    
    [StringLength(200)]
    public string DepartmentName {get ; set;}
    
    [StringLength(200)]
    public string ProjectName {get ; set;}
    
    [StringLength(36)]
    public string ProjectAcronym { get; set; }
    
    [StringLength(200)]
    public string Status { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    [StringLength(200)]
    public string CreatedBy { get; set; }
    [StringLength(200)]
    public string UpdatedBy { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
    
    public static readonly string STATUS_CREATE = "create";
    public static readonly string STATUS_INVITE = "invite";
    public static readonly string STATUS_PROVISIONED = "provisioned";
    public static readonly string STATUS_LOGGED_IN = "logged in";
    
    public Datahub_Registration_Request()
    {
        LinkId = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = STATUS_CREATE;
    }
}