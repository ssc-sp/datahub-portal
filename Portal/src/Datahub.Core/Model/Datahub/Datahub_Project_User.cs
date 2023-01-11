using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Elemental.Components;

namespace Datahub.Core.Model.Datahub;

public class Datahub_Project_User {

    [AeFormIgnore]
    [Key]

    public int ProjectUser_ID { get; set; }

    [StringLength(200)]
    public string User_ID { get; set; }
    public DateTime? Approved_DT { get; set; }

    public string ApprovedUser { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsDataApprover { get; set; }
    
    public Datahub_Project Project { get; set; }

    [StringLength(200)]
    public string User_Name {  get; set; }

    [Timestamp]
    public byte[] Timestamp { get; set; }

}