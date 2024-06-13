using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class ProjectUsageUpdateSender(ISendEndpointProvider sendEndpointProvider)
    : QueueMessageSender<ProjectUsageUpdateMessage>(sendEndpointProvider)
{

    protected override string ConfigPathOrQueueName => QueueConstants.ProjectUsageUpdateQueueName;
}
