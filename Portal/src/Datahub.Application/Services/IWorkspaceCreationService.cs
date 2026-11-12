using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Users;
using Datahub.Metadata.Model;

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
    /// <summary>
    /// Creates a workspace and requests its provisioning.
    /// </summary>
    /// <returns>True if the workspace was created; false if creation failed.</returns>
    /// <exception cref="Exceptions.NoSubnetGroupAvailableException">
    /// The workspace is Protected B and no subnet group is available. Nothing is saved.
    /// </exception>
    public Task<bool> CreateWorkspaceAsync(string projectName, string acronym, string organization, int? gcHostingDetailsId = null, decimal? budget = null, ClassificationType classification = ClassificationType.Unclassified);

    /// <inheritdoc cref="CreateWorkspaceAsync(string, string, string, int?, decimal?, ClassificationType)"/>
    public Task<bool> CreateWorkspaceAsync(string projectName, string organization, int? gcHostingDetailsId = null, ClassificationType classification = ClassificationType.Unclassified);
    
    public Task SaveWorkspaceCreationDetailsAsync(string projectAcronym, string? interestedFeatures = null);

    /// <summary>
    /// This is temporary until all existing workspaces have this configured
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="isProtectedB"></param> 
    /// <returns></returns>
    public Task CreateNewTemplateWorkspaceResourceAsync(int projectId, bool isProtectedB);
    
    /// <summary>
    /// This is temporary until all existing workspaces have this configured
    /// </summary>
    /// <param name="projectAcronym"></param>
    /// <returns></returns>
    public Task CreateNewTemplateWorkspaceResourceAsync(string projectAcronym);

    public Task SaveWorkspaceMetadataFromGCHostingDetails(string projectAcronym, GCHostingWorkspaceDetails workspaceDetails);
}
