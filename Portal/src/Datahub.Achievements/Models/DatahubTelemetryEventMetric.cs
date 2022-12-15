using System.ComponentModel.DataAnnotations;

namespace Datahub.Achievements.Models;

public class DatahubTelemetryEventMetric
{
    [Key]
    public string? Name { get; set; }
    public int Value { get; set; }
}