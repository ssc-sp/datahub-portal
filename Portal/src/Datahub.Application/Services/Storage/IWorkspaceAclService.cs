using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services.Storage;

/// <summary>
/// Service for managing ACLs on Azure Data Lake Storage Gen2
/// </summary>
public interface IWorkspaceAclService
{
    /// <summary>
    /// Applies permissions to all workspace members for a specific file or directory
    /// </summary>
    /// <param name="workspaceAcronym">The workspace acronym</param>
    /// <param name="filePath">The path to the file or directory (relative to container root)</param>
    /// <param name="permissions">Permissions to apply (e.g., "r--" for read, "r-x" for read+execute, "rwx" for full)</param>
    /// <param name="recursive">If true, applies ACLs recursively to all children</param>
    /// <returns>Number of items updated</returns>
    Task<int> ApplyWorkspaceMemberAclsAsync(string workspaceAcronym, string filePath, string permissions = "r-x", bool recursive = true);

    /// <summary>
    /// Gets all member user IDs (object IDs) for a workspace
    /// </summary>
    /// <param name="workspaceAcronym">The workspace acronym</param>
    /// <returns>List of user object IDs</returns>
    Task<List<string>> GetWorkspaceMemberIdsAsync(string workspaceAcronym);

    /// <summary>
    /// Gets workspace information by acronym
    /// </summary>
    /// <param name="workspaceAcronym">The workspace acronym</param>
    /// <returns>Workspace project or null if not found</returns>
    Task<Datahub_Project?> GetWorkspaceAsync(string workspaceAcronym);

    /// <summary>
    /// Removes all user ACLs from a path (keeps only owner/group/other)
    /// </summary>
    /// <param name="workspaceAcronym">The workspace acronym</param>
    /// <param name="filePath">The path to the file or directory</param>
    /// <param name="recursive">If true, removes ACLs recursively</param>
    /// <returns>Number of items updated</returns>
    Task<int> RemoveAllUserAclsFromPathAsync(
        string workspaceAcronym,
        string filePath,
        bool recursive = false);

    /// <summary>
    /// Simulates successful AV scan by updating blob metadata to trigger ACL function
    /// </summary>
    /// <param name="workspaceAcronym">The workspace acronym</param>
    /// <param name="filePath">The path to the file or directory</param>
    /// <returns>Number of items updated</returns>
    Task<int> SimulateScanSuccessAsync(string workspaceAcronym, string filePath);
}
