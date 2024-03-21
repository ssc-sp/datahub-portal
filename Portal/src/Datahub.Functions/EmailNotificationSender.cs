using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MailKit.Net.Smtp;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text.Json;

namespace Datahub.Functions;

public class EmailNotificationSender
{
    private readonly ILogger _logger;
    private readonly AzureConfig _config;
    private readonly QueuePongService _pongService;
    private readonly IPublishEndpoint _publishEndpoint;

    public EmailNotificationSender(ILoggerFactory loggerFactory, AzureConfig config,
         IPublishEndpoint publishEndpoint,
        QueuePongService pongService)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationSender>();
        _config = config;
        _pongService = pongService;
        _publishEndpoint = publishEndpoint;
    }

    [Function("EmailNotificationSender")]
    public async Task Run([QueueTrigger("%QueueEmailNotification%", Connection = "DatahubStorageConnectionString")] string requestMessage)
    {
        // test for ping
        if (await _pongService.Pong(requestMessage))
            return;

        // deserialize message
        var message = JsonSerializer.Deserialize<EmailRequestMessage>(requestMessage);
        if (message is null || !message.IsValid)
        {
            _logger.LogError($"Invalid message received: \n{requestMessage}");
            return;
        }

        // setting only used in development
        if (_config.Email.DumpMessages)
        {
            _logger.LogInformation($"No email sent: Dumping messages!");
            return;
        }

        await _publishEndpoint.Publish(message);
    }

}
