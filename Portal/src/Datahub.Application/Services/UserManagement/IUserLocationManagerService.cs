using Datahub.Core.Model.Users;

namespace Datahub.Application.Services.UserManagement;

public interface IUserLocationManagerService
{
    Task RegisterNavigation(UserRecentLink link);
    Task<ICollection<UserRecentLink>> GetRecentLinks(PortalUser user, int maxRecentLinks);
}