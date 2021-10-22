using System.Collections.Generic;
using System.Threading.Tasks;
using MimeKit;

namespace Datahub.Core.Services
{
    public interface IEmailNotificationService
    {
        Task<string> RenderTestTemplate();
        Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T: Microsoft.AspNetCore.Components.IComponent;
        Task SendEmailMessage(string subject, string body, string userIdOrAddress, string recipientName = null, bool isHtml = true);
        Task SendEmailMessage(string subject, string body, IList<string> userIdsOrAddresses, bool isHtml = true);
        bool IsDevTestMode();
        IList<MailboxAddress> TestUsernameEmailConversion(IList<(string address, string name)> recipients);
        string BuildAppLink(string contextUrl);

        Task SendServiceCreationRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendServiceAccessRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendServiceAccessGrantedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendServiceCreationRequestApprovedIndividual(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendServiceCreationGroupNotification(string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendAccessRevokedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendApplicationCompleteNotification(LanguageTrainingParameters parameters);
        Task SendLanguageSchoolDecision(LanguageTrainingParameters parameters);
        Task SendOnboardingConfirmations(OnboardingParameters parameters);
    }

    public record class DatahubProjectInfo(string ProjectNameEn, string ProjectNameFr, string ProjectCode);
}