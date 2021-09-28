using System.Collections.Generic;
using System.Threading.Tasks;
using MimeKit;

namespace NRCan.Datahub.Shared.Services
{
    public interface IEmailNotificationService
    {
        Task<string> RenderTestTemplate();
        Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T: Microsoft.AspNetCore.Components.IComponent;
        Task SendEmailMessage(string subject, string body, string userIdOrAddress, string recipientName = null, bool isHtml = true);
        Task SendEmailMessage(string subject, string body, IList<string> userIdsOrAddresses, bool isHtml = true);
        bool IsDevTestMode();
        IList<MailboxAddress> TestUsernameEmailConversion(IList<(string address, string name)> recipients);

        Task SendServiceCreationRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendServiceAccessRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendServiceAccessGrantedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendServiceCreationRequestApprovedIndividual(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendServiceCreationGroupNotification(string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendAccessRevokedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendApplicationCompleteNotification(LanguageTrainingParameters parameters);
    }
}