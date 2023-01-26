using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Microsoft.Graph;
using MimeKit;

namespace Datahub.Core.Services.Notification;

public interface IEmailNotificationService
{
    Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T : Microsoft.AspNetCore.Components.IComponent;
    Task SendEmailMessage(string subject, string body, string userIdOrAddress, string recipientName = null, bool isHtml = true);
    Task SendEmailMessage(string subject, string body, IList<string> userIdsOrAddresses, bool isHtml = true);
    Task SendEmailMessage(string subject, string body, List<DatahubEmailRecipient> recipients, bool isHtml = true);
    bool IsDevTestMode();
    string BuildAppLink(string contextUrl);
    Task<IList<MailboxAddress>> BuildRecipientList(IList<string> userIdsOrAddresses);
    Task<MailboxAddress> BuildRecipient(string userIdOrAddress, string recipientName = null);
    Task EmailErrorToDatahub(string subject, string fromUser, string message, string appInsightsMessage, string stackTrace);


}

public record DatahubProjectInfo(string ProjectNameEn, string ProjectNameFr, string ProjectCode);

public record DatahubEmailRecipient(string Name, string Address);