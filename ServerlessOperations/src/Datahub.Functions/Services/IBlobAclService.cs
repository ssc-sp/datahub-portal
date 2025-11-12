namespace Datahub.Functions.Services;

/// <summary>
/// Service for managing blob ACLs (Access Control Lists) in Azure Data Lake Storage Gen2
/// </summary>
public interface IBlobAclService
{
    /// <summary>
    /// Grants read access to all members of a workspace for a specific blob
    /// </summary>
    /// <param name="blobUri">The URI of the blob to update ACLs for</param>
    /// <param name="workspaceAcronym">The acronym of the workspace whose members should get access</param>
    /// <returns>Task representing the async operation</returns>
    Task GrantWorkspaceMembersReadAccessAsync(Uri blobUri, string workspaceAcronym);
}
