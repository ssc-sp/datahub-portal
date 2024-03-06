﻿namespace Datahub.Core.Model.Projects;

public class ProjectStorage
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public double AverageCapacity { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string CloudProvider { get; set; } = "azure";
}