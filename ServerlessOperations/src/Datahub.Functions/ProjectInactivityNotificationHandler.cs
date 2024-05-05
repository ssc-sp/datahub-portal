using Datahub.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Datahub.Infrastructure.Services.Queues;
using MassTransit;
using Azure.Messaging.ServiceBus;

namespace Datahub.Functions;

public class ProjectInactivityNotificationHandler
{
    private readonly ILogger _logger;
    private readonly IMessageReceiver receiver;

    public ProjectInactivityNotificationHandler(ILoggerFactory loggerFactory, IMessageReceiver receiver)
    {
        _logger = loggerFactory.CreateLogger<ProjectInactivityNotificationHandler>();
        this.receiver = receiver;
    }

    [Function("ProjectInactivityNotificationHandler")]
    public Task Run([ServiceBusTrigger(QueueConstants.QUEUE_PROJECT_INACTIVITY, Connection = "MassTransit:AzureServiceBus:ConnectionString")] ServiceBusReceivedMessage requestMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received message from '{QueueConstants.QUEUE_PROJECT_INACTIVITY}.");
        return receiver.HandleConsumer<ProjectInactivityNotificationConsumer>(QueueConstants.QUEUE_PROJECT_INACTIVITY, requestMessage, cancellationToken);
    }
 }
