using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elemental.Components;

namespace Datahub.Core.EFCore;

public class Datahub_Project_Access_Request
{
    [Key]
    public int Request_ID { get; set; }

    [Required]
    [StringLength(200)]
    public string User_Name { get; set; }

    [StringLength(200)]
    public string User_ID { get; set; }

    public bool Databricks { get; set; }
    public bool PowerBI { get; set; }
    public bool WebForms { get; set; }

    [NotMapped]
    [AeFormIgnore]
    public string RequestServiceType => (Databricks ? "Databricks" : (PowerBI ? "PowerBI" : "Web Forms"));

    public DateTime Request_DT { get; set; }

    public DateTime? Completion_DT { get; set; }

    public Datahub_Project Project { get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }
}