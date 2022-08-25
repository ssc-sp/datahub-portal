using System;
using System.ComponentModel.DataAnnotations;
using Elemental.Components;

namespace Datahub.Core.EFCore;

public class Datahub_ProjectComment
{
    [Key]
    [AeFormIgnore]

    public int Comment_ID { get; set; }

    public DateTime Comment_Date_DT { get; set; }

    public string Comment_NT { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }
    public Datahub_Project Project { get; set; }
}