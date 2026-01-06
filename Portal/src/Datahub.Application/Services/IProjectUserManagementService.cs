using Datahub.Application.Commands;
using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services;

public interface IProjectUserManagementService
{
    /// <summary>
    /// Batch updates the given project members in the project with the given acronym. Sends invites to users that are not already members of Datahub,
    /// and adds or updates the roles of users who are already members. Removes users that have been marked for removal.
    /// </summary>
    /// <param name="projectUserUpdateCommands"></param>
    /// <param name="projectUserAddUserCommands"></param>
    /// <param name="requesterUserId"></param>
    /// <returns></returns>
    Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands, List<ProjectUserAddEntraUserCommand> projectUserAddUserCommands, string requesterUserId);

    /// <summary>
    /// Gets all users in a project.
    /// </summary>
    Task<List<UserRoleLinks>> GetProjectUsersAsync(string projectAcronym);

    /// <summary>
    /// Get list of projects where user has any role assigned.
    /// </summary>
    Task<List<string>> GetProjectListForPortalUser(int portalUserId);

    /// <summary>
    /// Get project lead if defined.
    /// </summary>
    Task<UserRoleLinks?> GetProjectLeadAsync(string projectAcronym);

    /// <summary>
    /// Adds a message on the service bus to run a workspace sync
    /// </summary>
    /// <param name="projectAcronym"></param>
    /// <returns></returns>
    Task<bool> RunWorkspaceSync(string projectAcronym);

    /// <summary>
    /// Determines whether upload limits for external users are configured for the specified workspace.
    /// </summary>
    /// <param name="workspaceAcronym">The unique acronym identifying the workspace to check for external user upload limits. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if upload
    /// limits for external users are set for the workspace; otherwise, <see langword="false"/>.</returns>
    Task<bool> AreExternalUserUploadLimitsSet(string workspaceAcronym);

    /// <summary>
    /// Retrieves the upload limits for external users in the specified workspace.
    /// </summary>
    /// <param name="workspaceAcronym">The unique acronym identifying the workspace for which to retrieve external user upload limits. Cannot be null
    /// or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see
    /// cref="ExternalUserUploadLimit"/> object with the upload limits for external users in the specified workspace.</returns>
    Task<ExternalUserUploadLimit> GetExternalUserUploadLimits(string workspaceAcronym);

    /// <summary>
    /// Updates the upload limits for external users in the specified workspace.
    /// </summary>
    /// <param name="workspaceAcronym">The unique acronym identifying the workspace whose external user upload limits will be updated. Cannot be null
    /// or empty.</param>
    /// <param name="limits">An object containing the new upload limit settings to apply to external users. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateExternalUserUploadLimits(string workspaceAcronym, ExternalUserUploadLimit limits);
}

public record ExternalUserUploadLimit(long? MaximumFileSizeMB, int? MaximumFileCount);
