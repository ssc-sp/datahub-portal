using Datahub.Core.Model.Datahub;

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
    /// Whether the repository is public
    /// </summary>
    public bool IsPublic { get; set; }
    
    /// <summary>
    /// Project instance
    /// </summary>
    public Datahub_Project Project { get; set; }
}