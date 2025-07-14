namespace Datahub.Application.Services.Notification;

public interface IGCNotifyService
{
    Task SendNotification(string email, string templateId);
}
