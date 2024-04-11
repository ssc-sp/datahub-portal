using Azure.Core;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Repositories;

namespace Datahub.Application.Services.Notebooks;

public interface IDatabricksApiService
{
    /// <summary>
    /// List all repositories in the workspace that are shared from the Datahub Portal
    /// </summary>
    /// <param name="projectAcronym"></param>
    /// <returns></returns>
    public Task<List<ProjectRepository>> ListDisplayedWorkspaceRepositoriesAsync(string projectAcronym);

    /// <summary>
    /// List all repositories in the workspace that the user has access to through the Databricks API
    /// </summary>
    /// <param name="projectAcronym"></param>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    public Task<List<RepositoryInfoDto>> ListWorkspaceRepositoriesAsync(string projectAcronym, string accessToken);

    /// <summary>
    /// Update the workspace repository with the latest information from Databricks
    /// </summary>
    /// <param name="projectAcronym"></param>
    /// <param name="repositoryInfoDto"></param>
    /// <returns></returns>
    public Task<bool> UpdateWorkspaceRepository(string projectAcronym, RepositoryInfoDto repositoryInfoDto);


    /// <summary>
    /// Get Databricks Url for the project
    /// </summary>
    /// <param name="projectAcronym"></param> 
    /// <returns>Databricks Url</returns>
    public Task<string>GetDatabricsWorkspaceUrlAsync(string projectAcronym);

    /// <summary>
    /// Adds user as an admin to Databricks admin group
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="projectAcronym"></param> 
    /// <param name="user"></param>
    /// <returns></returns>
    public Task<bool> AddAdminToDatabricsWorkspaceAsync(AccessToken accessToken, string projectAcronym, PortalUser user);
}