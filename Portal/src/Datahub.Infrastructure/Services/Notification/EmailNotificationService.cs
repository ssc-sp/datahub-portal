using System.Net;
using System.Text;
using BlazorTemplater;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Templates;
using Datahub.Infrastructure.Services.Security;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Datahub.Infrastructure.Services.Notification;

public class EmailNotificationService : IEmailNotificationService
{

    public static readonly string EMAIL_CONFIGURATION_ROOT_KEY = "EmailNotification";

    public static readonly string USERNAME_TEMPLATE_KEY = "Username";
    public static readonly string SERVICE_TEMPLATE_KEY = "Service";
    public static readonly string DATA_PROJECT_TEMPLATE_KEY = "DataProject";

    private EmailConfiguration _config;

    private IStringLocalizer _localizer;

    private ILogger<EmailNotificationService> _logger;

    private IMSGraphService _graphService;

    private IServiceAuthManager _serviceAuthManager;

    public EmailNotificationService(
        IStringLocalizer localizer,
        IConfiguration config,
        ILogger<EmailNotificationService> logger,
        IMSGraphService graphService,
        IServiceAuthManager serviceAuthManager
    )
    {
        _localizer = localizer;
        _config = new EmailConfiguration();
        config.Bind(EMAIL_CONFIGURATION_ROOT_KEY, _config);
        if (_config.AppDomain is null)
        {
            logger.LogCritical("No Email Configuration available");
            _config = null;
        }
        _logger = logger;
        _graphService = graphService;
        _serviceAuthManager = serviceAuthManager;
    }

    

    public async Task<MailboxAddress> BuildRecipient(string userIdOrAddress, string recipientName = null)
    {
        if (Guid.TryParse(userIdOrAddress, out var parsedGuid))
        {
            // for a valid guid, try to get the corresponding user and use that info
            var user = await _graphService.GetUserAsync(userIdOrAddress, CancellationToken.None);
            if (user != null)
            {
                return CreateMailboxAddress(user.DisplayName, user.Mail);
            }
        }
        else if (userIdOrAddress.Contains('@'))
        {
            // if recipientName is provided, just use that along with the email address
            if (!string.IsNullOrEmpty(recipientName))
            {
                return CreateMailboxAddress(recipientName, userIdOrAddress);
            }
            else
            {
                // otherwise, try to lookup the user to get the name
                var userId = await _graphService.GetUserIdFromEmailAsync(userIdOrAddress, CancellationToken.None);
                // no need to null check userId, as GetUser has its own null check
                var user = await _graphService.GetUserAsync(userId, CancellationToken.None);

                // even if we don't get a user object, we can still try to send to the address
                return CreateMailboxAddress(user?.DisplayName, userIdOrAddress);
            }
        }

        return null;
    }

    static MailboxAddress CreateMailboxAddress(string name, string address) => new(name, address?.ToLower());

    public async Task<IList<MailboxAddress>> BuildRecipientList(IList<string> userIdsOrAddresses)
    {

        List<MailboxAddress> mailList = new();
        foreach (var item in userIdsOrAddresses)
        {
            var s = await BuildRecipient(item);
            mailList.Add(s);
        }

        return mailList;

    }

    public async Task<string> RenderTemplate<T>(IDictionary<string, object> parameters = null) where T : Microsoft.AspNetCore.Components.IComponent
    {
        var templater = new Templater();
        templater.AddService<IStringLocalizer>(_localizer);
        var html = templater.RenderComponent<T>(parameters);
        return await Task.FromResult(html);
    }

    public async Task SendEmailMessage(string subject, string body, string userIdOrAddress, string recipientName = null, bool isHtml = true)
    {
        if (_config is null)
        {
            _logger.LogCritical("Cannot send email - no configuration available");
            return;
        }

        var recipient = await BuildRecipient(userIdOrAddress, recipientName);
        if (recipient is not null)
        {
            var recipients = new List<MailboxAddress>() { recipient };
            await SendEmailMessage(subject, body, recipients, isHtml);
        }
    }

    public async Task SendEmailMessage(string subject, string body, IList<string> userIdsOrAddresses, bool isHtml = true)
    {
        if (_config is null)
        {
            _logger.LogCritical("Cannot send email - no configuration available");
            return;
        }

        var recipients = await BuildRecipientList(userIdsOrAddresses);
        await SendEmailMessage(subject, body, recipients, isHtml);
    }

    public async Task SendEmailMessage(string subject, string body, List<DatahubEmailRecipient> recipients, bool isHtml = true)
    {
        var mailboxRecipients = recipients.Select(r => CreateMailboxAddress(r.Name, r.Address)).ToList();
        await SendEmailMessage(subject, body, mailboxRecipients, isHtml);
    }

    public async Task EmailErrorToDatahub(string subject, string fromUser, string message, string appInsightsMessage, string stackTrace)
    {
        var adminEmails = _serviceAuthManager.GetProjectAdminsEmails(RoleConstants.DATAHUB_ADMIN_PROJECT);
        var parameters = new Dictionary<string, object>()
        {
            { "Date", $"{DateTime.UtcNow} UTC" },
            { "User", fromUser },
            { "Message", message },
            { "AppInsightsMessage", appInsightsMessage },
            { "StackTrace", stackTrace }
        };
        var bodyHtml = await RenderTemplate<GlobalErrorNotification>(parameters);
        await SendEmailMessage(subject, bodyHtml, adminEmails);
    }

    private static string BuildTestEmailOriginalRecipientsList(IEnumerable<MailboxAddress> recipients, bool isHtml)
    {
        var sb = new StringBuilder();
        if (isHtml)
        {
            sb.Append("<hr />");
            sb.Append("<strong>Original Recipients:</strong>");
            sb.Append("<ul>");
            foreach (var recipient in recipients)
            {
                sb.Append($"<li>{recipient.Name} - {recipient.Address}</li>");
            }
            sb.Append("</ul>");
        }
        else
        {
            const string nl = "\r\n";
            sb.Append(nl);
            sb.Append("-----");
            sb.Append(nl);
            sb.Append("Original recipients:");
            foreach (var recipient in recipients)
            {
                sb.Append(nl);
                sb.Append($"{recipient.Name} - {recipient.Address}");
            }
        }

        return sb.ToString();
    }

    private async Task SendEmailMessage(string subject, string body, IList<MailboxAddress> recipients, bool isHtml)
    {
        if (_config is null)
        {
            _logger.LogCritical("Cannot send email - no configuration available");
            return;
        }

        try
        {
            var validRecipients = recipients.Where(a => !string.IsNullOrWhiteSpace(a.Address)).ToHashSet();

            if (validRecipients.Count < 1)
            {
                _logger.LogWarning($"Cannot send '{subject}' - no valid recipients");
                return;
            }

            var msg = new MimeMessage();
            msg.From.Add(CreateMailboxAddress(_config.SenderName, _config.SenderAddress));

            if (IsDevTestMode())
            {
                var devEmail = await BuildRecipient(_config.DevTestEmail);
                msg.To.Add(devEmail);

                body += BuildTestEmailOriginalRecipientsList(validRecipients, isHtml);
            }
            else
            {
                msg.To.AddRange(validRecipients);
            }

            msg.Subject = subject;
            var bodyPart = new TextPart(isHtml ? "html" : "plain")
            {
                Text = body
            };
            msg.Body = bodyPart;

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_config.SmtpHost, _config.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(new NetworkCredential(_config.SmtpUsername, _config.SmtpPassword));

                await smtpClient.SendAsync(msg);
                await smtpClient.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            foreach (var item in recipients)
            {
                _logger.LogError(ex, $"Unable to send email to: {item.Name} with subject: {subject}.");
            }
        }
    }

    public bool IsDevTestMode()
    {
        return _config.DevTestMode;
    }
    public string BuildAppLink(string contextUrl)
    {
        var ub = new UriBuilder(_config.AppDomain);
        ub.Path = contextUrl;
        return ub.ToString();
    }

}

