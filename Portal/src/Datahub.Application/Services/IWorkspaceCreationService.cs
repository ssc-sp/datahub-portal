using Datahub.Core.Model.Achievements;

namespace Datahub.Application.Services;

public interface IWorkspaceCreationService
{
    public Task<bool> AcronymExists(string acronym);
    public Task<string> GenerateWorkspaceAcronymAsync(string projectName);
    public Task<string> GenerateWorkspaceAcronymAsync(string projectName, IEnumerable<string> existingAcronyms);
    //token needs to be acquired by component so that exception handling can be done there
    //(handling exception causes force refresh through navigation manager)
    public Task CreateWorkspaceCloudHostingEndPointAsync(string projectName, string acronym, string organization, PortalUser portalUser);
    public Task<bool> CreateWorkspaceAsync(string projectName, string acronym, string organization); 
    public Task<bool> CreateWorkspaceAsync(string projectName, string organization);
    
    public Task SaveWorkspaceCreationDetailsAsync(string projectAcronym, string? interestedFeatures = null);

    /// <summary>
    /// This is temporary until all existing workspaces have this configured
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public Task CreateNewTemplateWorkspaceResourceAsync(int projectId);
    
    /// <summary>
    /// This is temporary until all existing workspaces have this configured
    /// </summary>
    /// <param name="projectAcronym"></param>
    /// <returns></returns>
    public Task CreateNewTemplateWorkspaceResourceAsync(string projectAcronym);
}