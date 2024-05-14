using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MailKit.Net.Smtp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Datahub.Functions.Extensions;
using Datahub.Shared.Configuration;

namespace Datahub.Functions;

public class EmailNotificationHandler
{
    private readonly ILogger _logger;
    private readonly AzureConfig _config;
    private readonly QueuePongService _pongService;

    public EmailNotificationHandler(ILoggerFactory loggerFactory, AzureConfig config, QueuePongService pongService)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationHandler>();
        _config = config;
        _pongService = pongService;
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
        if (message is null || !message.IsValid)
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
}