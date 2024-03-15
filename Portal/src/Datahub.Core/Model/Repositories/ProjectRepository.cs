using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Repositories;

public sealed class ProjectRepository
{
    /// <summary>
    /// Gets or sets project Repository Id (PK)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets project table (FK)
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets repository URL
    /// </summary>
    public string RepositoryUrl { get; set; }

    /// <summary>
    /// Gets or sets the location of the repository in the workspace
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the repository provider (ie. GitHub)
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets the repository branch
    /// </summary>
    public string Branch { get; set; }

    /// <summary>
    /// Gets or sets the repository head commit id
    /// </summary>
    public string HeadCommitId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether whether the repository is public
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets project instance
    /// </summary>
    public Datahub_Project Project { get; set; }
}