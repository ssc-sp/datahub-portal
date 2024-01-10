using MimeKit;

namespace Datahub.Application.Services.Notification;

public class EmailConfiguration
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public string SenderName { get; set; }
    public string SenderAddress { get; set; }
    public string AppDomain { get; set; }
    public bool DevTestMode { get; set; }
    public string DevTestEmail { get; set; }
}

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

public record DatahubEmailRecipient(string Name, string Address);