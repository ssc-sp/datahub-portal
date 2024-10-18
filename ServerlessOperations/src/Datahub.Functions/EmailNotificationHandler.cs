using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MailKit.Net.Smtp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MimeKit;
using Azure.Messaging.ServiceBus;
using Datahub.Functions.Extensions;
using Datahub.Shared.Configuration;
using Datahub.Functions.Services;
using Newtonsoft.Json;

namespace Datahub.Functions;

public class EmailNotificationHandler
{
    private readonly ILogger _logger;
    private readonly AzureConfig _config; 
    private readonly IEmailService _emailService;

    public EmailNotificationHandler(
        ILoggerFactory loggerFactory, 
        AzureConfig config,  
        IEmailService emailService)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationHandler>();
        _config = config; 
        _emailService = emailService;
    }

    [Function("EmailNotificationHandler")]
    public async Task Run(
        [ServiceBusTrigger(QueueConstants.EmailNotificationQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        // test for ping
        // if (await _pongService.Pong(serviceBusReceivedMessage.Body.ToString()))
            // return;

        // check mail configuration
        if (!_config.Email.IsValid)
        {
            _logger.LogError($"Invalid mail configuration!");
            return;
        }

        // deserialize message
        var message = await serviceBusReceivedMessage.DeserializeAndUnwrapMessageAsync<EmailRequestMessage>();
        if (message is null || !message.IsValid || message.Body == null)
        {
            _logger.LogError($"Invalid message received: \n{serviceBusReceivedMessage.Body}");
            return;
        }

        // setting only used in development
        if (_config.Email.DumpMessages)
        {
            _logger.LogInformation($"No email sent: Dumping messages!");
            return;
        }

        // handle user re-activation 
        if (message.Subject == "FSDH Account Reactivation Notification")
        {
            var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(message.Body);
            message = GetEmailRequestMessage(message.To, parameters);
        }

        // send message
        await SendMessage(message);
    }

    public EmailRequestMessage GetEmailRequestMessage(List<string> contacts, Dictionary<string, string>? parameters)
    {
        if (parameters == null) return null;
        List<string> bcc = new() { GetNotificationCCAddress()};
        
        Dictionary<string, string> subjectArgs = new()
            {
                { "{{ws}}", parameters["WorkspaceAcronym"] }
            };

        Dictionary<string, string> bodyArgs = new()
            {
                { "{ws}", parameters["WorkspaceAcronym"] },
                { "{leadName}", parameters["LeadName"] },
                { "{userName}", parameters["UserName"] }
        };

        var email = _emailService.BuildEmail("user_reactivation.html", contacts, bcc, bodyArgs,
            subjectArgs);

        return email;
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

            await smtpClient.ConnectAsync(_config.Email.SmtpHost, _config.Email.SmtpPort,
                MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(new System.Net.NetworkCredential(_config.Email.SmtpUsername,
                _config.Email.SmtpPassword));

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

    private string GetNotificationCCAddress()
    {
        return _config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
    }
}