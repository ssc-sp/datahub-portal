namespace Datahub.Application.Services.Notification;

public interface IGCNotifyService
{
    Task SendNotification(string postDataJson);
    Task SendAccountCreatedNotification(string email);
    Task SendAccountDeletionNoticeNotification(string email, string daysSince, string daysUntil);
    Task SendAccountLockingNoticeNotification(string email, string daysSince, string daysUntil);
    Task SendWorkspaceCostNotification(string email, string perc);
}
