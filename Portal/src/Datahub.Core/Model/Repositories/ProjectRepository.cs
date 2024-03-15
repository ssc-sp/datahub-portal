using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Repositories;

public sealed class ProjectRepository
{
    /// <summary>
    /// Project Repository Id (PK)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Project table (FK)
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Repository URL
    /// </summary>
    public string RepositoryUrl { get; set; }

    /// <summary>
    /// The location of the repository in the workspace
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The repository provider (ie. GitHub)
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// The repository branch
    /// </summary>
    public string Branch { get; set; }

    /// <summary>
    /// The repository head commit id
    /// </summary>
    public string HeadCommitId { get; set; }

    /// <summary>
    /// Whether the repository is public
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Project instance
    /// </summary>
    public Datahub_Project Project { get; set; }
}