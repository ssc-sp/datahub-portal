using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions;

public class EmailNotificationHandler
{
    private readonly ILogger _logger;
    private readonly AzureConfig _config;

    public EmailNotificationHandler(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationHandler>();
        _config = new(configuration);
    }

    [Function("EmailNotificationHandler")]
    public async Task Run([QueueTrigger("email-notifications", Connection = "DatahubStorageConnectionString")] string requestMessage)
    {
        // check mail configuration
        if (!_config.Email.IsValid)
        {
            _logger.LogError($"Invalid mail configuration!");
            return;
        }
        
        // deserialize message
        var message = JsonSerializer.Deserialize<EmailRequestMessage>(requestMessage);
        if (message is null || !message.IsValid)
        {
            _logger.LogError($"Invalid message received: \n{requestMessage}");
            return;
        }

        // send message
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
