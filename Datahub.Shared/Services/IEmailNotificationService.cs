using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public interface IEmailNotificationService
    {
        Task<string> RenderTestTemplate();
        Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T: Microsoft.AspNetCore.Components.IComponent;
        Task SendEmailMessage(string subject, string body, string recipientAddress, string recipientName = null, bool isHtml = true);
        Task SendEmailMessage(string subject, string body, IList<string> recipientAddresses, bool isHtml = true);
        bool IsDevTestMode();

        Task SendServiceCreationRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendServiceAccessRequestNotification(string username, string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendServiceAccessGrantedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendServiceCreationRequestApprovedIndividual(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
        Task SendServiceCreationGroupNotification(string serviceName, DatahubProjectInfo projectInfo, IList<string> recipients);
        Task SendAccessRevokedNotification(string serviceName, DatahubProjectInfo projectInfo, string recipientAddress, string recipientName = null);
    }
}