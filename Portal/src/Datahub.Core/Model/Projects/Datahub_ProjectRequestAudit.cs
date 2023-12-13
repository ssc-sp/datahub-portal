using System;
using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.Model.Projects;

public class Datahub_ProjectRequestAudit
{
    [Key]
    public int Id { get; set; }

    public DateTime RequestedDateTime { get; set; }
    public DateTime? CompletedDateTime { get; set; }

    [StringLength(100)]
    public string RequestType { get; set; }
        
    [StringLength(100)]
    public string UserEmail { get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }

    public Datahub_Project Project { get; set; }
}