using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class BugReportMessageSender(ISendEndpointProvider sendEndpointProvider)
        : QueueMessageSender<BugReportMessage>(sendEndpointProvider)
    {
        protected override string ConfigPathOrQueueName => QueueConstants.BugReportQueueName;
    }
}