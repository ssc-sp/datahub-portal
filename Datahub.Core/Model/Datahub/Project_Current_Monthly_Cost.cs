using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datahub.Core.EFCore;

public class Project_Current_Monthly_Cost
{
    [Key] public int Id { get; set; }
    public decimal TotalCost { get; set; }

    // ReSharper disable once InconsistentNaming
    public decimal TotalCostUSD { get; set; }
    public DateTime UpdateDate { get; set; }
    public string ProjectAcronym { get; set; }
}
    