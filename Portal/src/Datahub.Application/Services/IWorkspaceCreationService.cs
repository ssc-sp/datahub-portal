using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Onboarding;

namespace Datahub.Application.Services;

public interface IWorkspaceCreationService
{
    public Task<bool> AcronymExists(string acronym);
    public Task<IEnumerable<GCHostingWorkspaceDetails>> GetGCHostingWorkspaceDetailsForUser(PortalUser user);
    public Task<IEnumerable<GCHostingWorkspaceDetails>> GetGCHostingWorkspaceDetailsForCurrentUser();
    public Task<IEnumerable<GCHostingWorkspaceDetails>> GetAllGCHostingWorkspaceDetails();
    public Task<string> GenerateWorkspaceAcronymAsync(string projectName);
    public Task<string> GenerateWorkspaceAcronymAsync(string projectName, IEnumerable<string> existingAcronyms);
    //token needs to be acquired by component so that exception handling can be done there
    //(handling exception causes force refresh through navigation manager)
    public Task CreateWorkspaceCloudHostingEndPointAsync(string projectName, string acronym, string organization, PortalUser portalUser, decimal budget, string cbrId);
    public Task<bool> CreateWorkspaceAsync(string projectName, string acronym, string organization, int? gcHostingDetailsId = null, decimal? budget = null); 
    public Task<bool> CreateWorkspaceAsync(string projectName, string organization, int? gcHostingDetailsId = null);
    
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

    public Task SaveWorkspaceMetadataFromGCHostingDetails(string projectAcronym, GCHostingWorkspaceDetails workspaceDetails);

    /// <summary>
    /// Returns a single GC Hosting workspace details record by its id.
    /// Optionally includes child workspaces and their credits.
    /// </summary>
    Task<GCHostingWorkspaceDetails?> GetGCHostingWorkspaceDetailsAsync(int id, bool includeWorkspaces = false, bool includeWorkspaceCredits = false);

    /// <summary>
    /// Returns the parent GC Hosting workspace details object for the workspace identified by the acronym.
    /// </summary>
    Task<GCHostingWorkspaceDetails?> GetParentHostingDetailsForWorkspaceAsync(string projectAcronym, bool includeWorkspaces = false, bool includeWorkspaceCredits = false);
}