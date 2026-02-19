namespace Datahub.Core.Services;

public class NotificationPropagationService : IPropagationService
{
    public event Func<IEnumerable<string>, Task>? UpdateSystemNotifications;

    public async Task PropagateSystemNotificationUpdate(IEnumerable<string> userIds)
    {
        if (UpdateSystemNotifications != null)
        {
            await UpdateSystemNotifications.Invoke(userIds);
        }
    }
}
