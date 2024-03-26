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
    Task<bool> ProcessProjectUserCommandsAsync(List<ProjectUserUpdateCommand> projectUserUpdateCommands, List<ProjectUserAddUserCommand> projectUserAddUserCommands, string requesterUserId);

    /// <summary>
    /// Gets all users in a project.
    /// </summary>
    Task<List<Datahub_Project_User>> GetProjectUsersAsync(string projectAcronym);
}