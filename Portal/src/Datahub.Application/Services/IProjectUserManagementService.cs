using Datahub.Core.Model.Datahub;

namespace Datahub.Application.Services;

public interface IProjectUserManagementService
{
    Task AddUserToProject(string projectAcronym, string userGraphId);
    Task RemoveUserFromProject(string projectAcronym, string userGraphId);
    Task UpdateUserInProject(string projectAcronym, Datahub_Project_User user);

    Task<IEnumerable<Datahub_Project_User>> GetProjectUsers(string projectAcronym);
}