using Datahub.Core.Model.Datahub;
using Datahub.Core.Data.Project;

namespace Datahub.Application.Services;

public interface IProjectUserManagementService
{
    /// <summary>
    /// Batch updates the given project members in the project with the given acronym. Sends invites to users that are not already members of Datahub,
    /// and adds or updates the roles of users who are already members. Removes users that have been marked for removal.
    /// </summary>
    /// <param name="projectAcronym">The acronym of the project to update.</param>
    /// <param name="projectMembers">The list of project members to update in the project.</param>
    /// <returns>
    /// An enumerable list of project members that could not be invited to DataHub.
    /// An empty list if all members were invited successfully or no invites were needed to be sent.
    /// </returns>
    Task<IEnumerable<ProjectMember>> BatchUpdateUsersInProject(string projectAcronym, IEnumerable<ProjectMember> projectMembers);

    /// <summary>
    /// Adds a single user to a project with the default role of collaborator.
    /// </summary>
    Task AddUserToProject(string projectAcronym, string userGraphIds);

    /// <summary>
    /// Adds multiple users to a project with the default role of collaborator.
    /// </summary>
    Task AddUsersToProject(string projectAcronym, IEnumerable<string> userGraphId);

    /// <summary>
    /// Removes a user from a project.
    /// </summary>
    Task RemoveUserFromProject(string projectAcronym, string userGraphId);

    /// <summary>
    /// Updates the role of a user in a project.
    /// </summary>
    Task UpdateUserInProject(string projectAcronym, ProjectMember projectMember);

    /// <summary>
    /// Gets all users in a project.
    /// </summary>
    Task<IEnumerable<Datahub_Project_User>> GetUsersFromProject(string projectAcronym);
}