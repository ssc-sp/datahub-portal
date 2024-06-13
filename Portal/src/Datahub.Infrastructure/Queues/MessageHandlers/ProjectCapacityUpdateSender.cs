using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class ProjectCapacityUpdateSender(ISendEndpointProvider sendEndpointProvider)
    : QueueMessageSender<ProjectCapacityUpdateMessage>(sendEndpointProvider)
{

    protected override string ConfigPathOrQueueName => QueueConstants.ProjectCapacityUpdateQueueName;
}
