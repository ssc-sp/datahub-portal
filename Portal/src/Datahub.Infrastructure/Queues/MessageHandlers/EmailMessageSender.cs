using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class EmailMessageSender(ISendEndpointProvider sendEndpointProvider)
    : QueueMessageSender<EmailRequestMessage>(sendEndpointProvider)
{
    protected override string ConfigPathOrQueueName => QueueConstants.EmailNotificationQueueName;
}