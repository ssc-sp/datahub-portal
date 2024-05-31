using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class PongMessageHandler(ISendEndpointProvider sendEndpointProvider)
    : QueueMessageSender<PongMessage>(sendEndpointProvider)
{

    protected override string ConfigPathOrQueueName => QueueConstants.PongQueueName;
}
