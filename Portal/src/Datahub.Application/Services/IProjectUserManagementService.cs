using Datahub.Core.Model.Datahub;
using Datahub.Core.Data.Project;

namespace Datahub.Application.Services;

public interface IProjectUserManagementService
{
    Task AddUserToProject(string projectAcronym, string userGraphIds);
    Task AddUsersToProject(string projectAcronym, IEnumerable<string> userGraphId);
    Task RemoveUserFromProject(string projectAcronym, string userGraphId);
    Task UpdateUserInProject(string projectAcronym, ProjectMember projectMember);

    Task<IEnumerable<Datahub_Project_User>> GetUsersFromProject(string projectAcronym);
}