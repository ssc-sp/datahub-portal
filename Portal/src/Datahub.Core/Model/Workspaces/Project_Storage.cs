namespace Datahub.Core.Model.Projects;

/// <summary>
/// Represents the storage details for a Datahub project.
/// </summary>
public class Project_Storage
{
    /// <summary>
    /// Gets or sets the unique identifier for this storage entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the associated project.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the average capacity usage for the project, in gigabytes.
    /// </summary>
    public double AverageCapacity { get; set; }

    /// <summary>
    /// Gets or sets the date (in UTC) when the record was created or updated.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the name of the cloud provider, defaulting to "azure".
    /// </summary>
    public string CloudProvider { get; set; } = "azure";
}
