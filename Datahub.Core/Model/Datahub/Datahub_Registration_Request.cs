using System;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.EFCore;

public class Datahub_Registration_Request
{
    public int Id { get; set; }
    public string Email { get; set; }
    public Guid? LinkId { get; set; }
    
    public string RequestedProjectAcronym { get; set; }
    
    // create
    // invite
    // provisioned
    // logged in
    public string Status { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}