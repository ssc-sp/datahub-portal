using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore;

public class Datahub_Registration_Request
{
    public int Id { get; set; }
    
    [StringLength(254)]
    public string Email { get; set; }
    
    [StringLength(36)]
    public Guid? LinkId { get; set; }
    
    [StringLength(36)]
    public string RequestedProjectAcronym { get; set; }
    
    // create
    // invite
    // provisioned
    // logged in
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
}