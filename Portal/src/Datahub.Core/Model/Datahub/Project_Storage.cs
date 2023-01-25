using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.Model.Datahub;

public enum Storage_Type
{
    AzureGen1, AzureGen2
}

public class Project_Storage
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string AccountName { get; set; }

    public Storage_Type Storage_Type { get; set; }
}

public class Project_Storage_Capacity
{
    public int ProjectId { get; set; }
    public Storage_Type Type { get; set; }
    public Datahub_Project Project { get; set; }
    public double UsedCapacity { get; set; }
    public DateTime? LastUpdated { get; set; }
}