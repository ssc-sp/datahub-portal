using MassTransit;
using Datahub.Infrastructure.Queues.Messages;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit; 
using Datahub.Application.Configuration;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Services;

public class EmailNotificationConsumer: IConsumer<EmailRequestMessage>
{
    private readonly ILogger _logger;
    private readonly EmailNotification _emailConfig;
    private readonly IConfiguration _config;

    public EmailNotificationConsumer(ILoggerFactory loggerFactory, IConfiguration config)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationConsumer>();
        _config = config;
        _emailConfig = new EmailNotification();
        _config.Bind("EmailNotification", _emailConfig);
    }
    public async Task Consume(ConsumeContext<EmailRequestMessage> context)
    {
        // check mail configuration
        if (!_emailConfig.IsValid)
        {
            _logger.LogError($"Invalid mail configuration!");
            return;
        }

        var message = context.Message;
        if (message is null || !message.IsValid)
        {
            _logger.LogError($"Invalid message received: \n{context}");
            return;
        }

        // setting only used in development
        if (_emailConfig.DumpMessages)
        {
            _logger.LogInformation($"No email sent: Dumping messages!");
            return;
        }
        await SendMessage(message);
    }
    private async Task SendMessage(EmailRequestMessage req)
    {
        try
        {
            using MimeMessage message = new();

            message.From.Add(new MailboxAddress(_emailConfig.SenderName, _emailConfig.SenderAddress));
            message.To.AddRange(req.To.Select(GetMailboxAddress));
            message.Cc.AddRange(req.CcTo.Select(GetMailboxAddress));
            message.Bcc.AddRange(req.BccTo.Select(GetMailboxAddress));

            message.Subject = req.Subject;

            //var builder = new BodyBuilder { HtmlBody = req.Body };
            // add attachments (see: builder.Attachments.Add())
            //message.Body = builder.ToMessageBody();

            message.Body = new TextPart("html")
            {
                Text = req.Body
            };

            using var smtpClient = new SmtpClient();

            await smtpClient.ConnectAsync(_emailConfig.SmtpHost, _emailConfig.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(new System.Net.NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword));

            await smtpClient.SendAsync(message);
            await smtpClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Fail to send email!");
            throw;
        }
    }

    static MailboxAddress GetMailboxAddress(string address) => new(address, address);

}
