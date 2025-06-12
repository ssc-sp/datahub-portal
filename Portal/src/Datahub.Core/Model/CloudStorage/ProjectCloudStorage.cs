using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.CloudStorage;

/// <summary>
/// Represents a cloud storage account associated with a Datahub workspace.
/// </summary>
public class ProjectCloudStorage
{
    /// <summary>
    /// Gets or sets the unique identifier for the workspace cloud storage.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the workspace this cloud storage belongs to.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the navigation property for the Datahub workspace.
    /// </summary>
    public virtual Datahub_Project Project { get; set; }

    /// <summary>
    /// Gets or sets the cloud storage provider (e.g., Azure Blob Storage).
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the name of the cloud storage account.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the connection data or relevant configuration for the cloud storage.
    /// This might include connection strings or other provider-specific data.
    /// </summary>
    public string ConnectionData { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the cloud storage is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the list of files associated with this cloud storage for open data publishing submissions.
    /// </summary>
    public IList<OpenDataPublishFile> PublishingSubmissionFiles { get; set; }
}
