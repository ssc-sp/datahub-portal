using System.ComponentModel.DataAnnotations;

namespace Datahub.Core.Model.Projects;

public class Datahub_Project_Costs
{
    [Key]
    public int ProjectCosts_ID { get; set; }

    public int Project_ID { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public double CadCost { get; set; }

    [StringLength(64)]
    public required string ServiceName { get; set; }

    [StringLength(5)]
    public string CloudProvider { get; set; } = "azure";
}