using MassTransit;
using Datahub.Infrastructure.Queues.Messages;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Datahub.Functions;

public class EmailNotificationConsumer: IConsumer<EmailRequestMessage>
{
    private readonly ILogger _logger;
    private readonly AzureConfig _config;

    public EmailNotificationConsumer(ILoggerFactory loggerFactory, AzureConfig config)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationConsumer>();
        _config = config; 
    }
    public async Task Consume(ConsumeContext<EmailRequestMessage> context)
    {
        // check mail configuration
        if (!_config.Email.IsValid)
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
        if (_config.Email.DumpMessages)
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

            message.From.Add(new MailboxAddress(_config.Email.SenderName, _config.Email.SenderAddress));
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

            await smtpClient.ConnectAsync(_config.Email.SmtpHost, _config.Email.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(new System.Net.NetworkCredential(_config.Email.SmtpUsername, _config.Email.SmtpPassword));

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
