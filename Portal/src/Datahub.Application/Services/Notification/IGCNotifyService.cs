using Datahub.Application.Configuration;

namespace Datahub.Application.Services.Notification;

public interface IGCNotifyService
{
    Task SendNotification(string postDataJson);
    Task SendAccountCreatedNotification(string email);
    Task SendAccountDeletionNoticeNotification(string email, string daysSince, string daysUntil);
    Task SendAccountLockingNoticeNotification(string email, string daysSince, string daysUntil);
    Task SendWorkspaceCostNotification(string email, string perc, string acro);
    Task SendDataHubErrorNotification(string errorMessage, string email);
    Task SendDatahubResourceDeletedNotification(string email, string resource, string resource_fr, string acro);
    string GetTemplateMappings(DatahubPortalConfiguration portalConfiguration);
    string GetTemplateId(string templateName, string mappingsJson);
}
