using Datahub.Infrastructure.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Datahub.Infrastructure.Services.Queues;
using MassTransit;
using Azure.Messaging.ServiceBus;

namespace Datahub.Functions;

public class ProjectCapacityUpdateHandler
{
    private readonly ILogger _logger;
    private readonly IMessageReceiver receiver;

    public ProjectCapacityUpdateHandler(ILoggerFactory loggerFactory, IMessageReceiver receiver)
    {
        _logger = loggerFactory.CreateLogger<ProjectCapacityUpdateHandler>();
        this.receiver = receiver;
    }

    [Function("ProjectCapacityUpdateHandler")]
    public Task Run([ServiceBusTrigger(QueueConstants.QUEUE_PROJECT_CAPACITY_UPDATE, Connection = "MassTransit:AzureServiceBus:ConnectionString")] ServiceBusReceivedMessage requestMessage, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Received message from '{QueueConstants.QUEUE_PROJECT_CAPACITY_UPDATE}.");
        return receiver.HandleConsumer<ProjectCapacityUpdateConsumer>(QueueConstants.QUEUE_PROJECT_CAPACITY_UPDATE, requestMessage, cancellationToken);
    }
}
