namespace Datahub.Core.Services;

public interface IPropagationService
{
    event Func<IEnumerable<string>, Task> UpdateSystemNotifications;
    Task PropagateSystemNotificationUpdate(IEnumerable<string> userIds);
}