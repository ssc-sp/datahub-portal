using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class ProjectUsageNotificationSender(ISendEndpointProvider sendEndpointProvider)
    : QueueMessageSender<ProjectUsageNotificationMessage>(sendEndpointProvider)
{
    protected override string ConfigPathOrQueueName =>
        QueueConstants.ProjectUsageNotificationQueueName;
}