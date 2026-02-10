using System.Threading;
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
    Task SendWelcomePackageNotification(string email);
    Task SendWorkspaceInactiveNotification(string email, string daysSince);
    Task SendBugReportNotification(string id, string title, string body, string email = "datasolutions-solutiondedonnees@ssc-spc.gc.ca");
    Task SendInfectedFileNotification(string email, string fileName, string workspace, string date);
    Task SendUserAccessRegrantedNotification(string email, string userName, string workspace);
    Task SendStorageScanSuccessEmailAsync(StorageScanNotificationHelper.StorageScanSuccessEventPayload payload, string? recipientEmail = null, CancellationToken cancellationToken = default);
    string GetTemplateMappings(DatahubPortalConfiguration portalConfiguration);
    string GetTemplateId(string templateName, string mappingsJson);
    Task<bool> CheckHealthAsync();
}