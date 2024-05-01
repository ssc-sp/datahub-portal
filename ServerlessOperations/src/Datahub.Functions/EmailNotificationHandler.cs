using Datahub.Application;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MailKit.Net.Smtp;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Extensions;
using MimeKit;
using System.Text.Json;
using Datahub.Infrastructure.Services.Queues;
using System.Threading;
using MassTransit;
using Azure.Messaging.ServiceBus;

namespace Datahub.Functions;

public class EmailNotificationHandler
{
    private readonly ILogger _logger;
    private readonly AzureConfig _config;
    private readonly QueuePongService _pongService;
    private readonly IMessageReceiver receiver;

    public EmailNotificationHandler(ILoggerFactory loggerFactory, AzureConfig config, QueuePongService pongService, IMessageReceiver receiver)
    {
        _logger = loggerFactory.CreateLogger<EmailNotificationHandler>();
        _config = config;
        _pongService = pongService;
        this.receiver = receiver;
    }

    [Function("EmailNotificationHandler")]
    public Task Run([ServiceBusTrigger(QueueConstants.QUEUE_EMAILS)] ServiceBusReceivedMessage requestMessage, CancellationToken cancellationToken)
    {
        return receiver.HandleConsumer<EmailNotificationConsumer>(QueueConstants.QUEUE_EMAILS, requestMessage, cancellationToken);
    }

    static MailboxAddress GetMailboxAddress(string address) => new(address, address); 
}
