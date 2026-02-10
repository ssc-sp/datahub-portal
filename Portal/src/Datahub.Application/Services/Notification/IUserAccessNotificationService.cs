using Datahub.Application.Services;

namespace Datahub.Application.Services.Notification;

public interface IUserAccessNotificationService
{
    Task NotifyAccessRegrantedAsync(UserLockStatus lockStatus);
}