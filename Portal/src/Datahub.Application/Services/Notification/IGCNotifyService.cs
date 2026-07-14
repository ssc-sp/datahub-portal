using System.Threading;
using Datahub.Application.Configuration;

namespace Datahub.Application.Services.Notification;

public interface IGCNotifyService
{
    public const string DEFAULT_MAILBOX = "datasolutions-solutiondedonnees@ssc-spc.gc.ca";
    Task SendNotification(string postDataJson);
    Task SendAccountCreatedNotification(string email);
    Task SendAccountDeletionNoticeNotification(string email, string daysSince, string daysUntil);
    Task SendAccountLockingNoticeNotification(string email, string daysSince, string daysUntil);
    Task SendWorkspaceCostNotification(string email, string perc, string acro);
    Task SendDataHubErrorNotification(string errorMessage, string email = DEFAULT_MAILBOX);
    Task SendDatahubResourceDeletedNotification(string email, string resource, string resource_fr, string acro);
    Task SendWelcomePackageNotification(string email);
    Task SendWorkspaceInactiveNotification(string email, string daysSince);
    Task SendExternalUserInviteNotification(string email, string externalUserName, string workspace, string inviterName, string invitationURL_en, string invitationURL_fr);
    Task SendBugReportNotification(string id, string title, string body, string email = DEFAULT_MAILBOX);
    Task SendInfectedFileNotification(string email, string fileName, string workspace, string date);
    Task SendUserAccessRegrantedNotification(string email, string userName, string workspace, string unlockedBy);
    string GetTemplateMappings();
    string GetTemplateId(string templateName, string mappingsJson);
    Task<bool> CheckHealthAsync();
}
