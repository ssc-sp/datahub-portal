using Datahub.Core.Model.UserTracking;

namespace Datahub.Application.Services.UserManagement;

public interface IUserLocationManagerService
{
    Task RegisterNavigation(UserRecentLink link);
    Task<ICollection<UserRecentLink>> GetRecentLinks(string userId, int maxRecentLinks);
}