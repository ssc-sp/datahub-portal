using Datahub.Core.Model.Datahub;
using Datahub.Core.Data.Project;

namespace Datahub.Application.Services;

public interface IProjectUserManagementService
{
    public Task BatchUpdateUsersInProject(string projectAcronym,
        IEnumerable<(string userGraphId, ProjectMemberRole role)> projectMembers);
    Task AddUserToProject(string projectAcronym, string userGraphIds);
    Task AddUsersToProject(string projectAcronym, IEnumerable<string> userGraphId);
    Task RemoveUserFromProject(string projectAcronym, string userGraphId);
    Task UpdateUserInProject(string projectAcronym, ProjectMember projectMember);

    Task<IEnumerable<Datahub_Project_User>> GetUsersFromProject(string projectAcronym);
}