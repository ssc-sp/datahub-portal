using System.Collections.Immutable;
using Datahub.Core.Model.Projects;

namespace Datahub.Application.Services.Security;

public interface IServiceAuthManager
{
    List<string> GetAllProjects();
    void SetViewingAsGuest(string userId, bool isGuest);
    bool GetViewingAsGuest(string userId);
    List<string> GetAdminProjectRoles(string userId);
    bool InvalidateAuthCache();
    Task<bool> IsProjectAdmin(string userid, string projectAcronym);
    List<string> GetProjectAdminsEmails(string projectAcronym);
    List<string> GetProjectMailboxEmails(string projectAcronym);
    Task<Dictionary<string, List<string>>> CheckCacheForAdmins();
    Task<ImmutableList<(Project_Role Role, Datahub_Project Project)>> GetUserAuthorizations(string userGraphId);
}