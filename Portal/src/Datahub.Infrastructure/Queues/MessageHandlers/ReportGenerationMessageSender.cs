using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class ReportGenerationMessageSender(ISendEndpointProvider sendEndpointProvider)
        : QueueMessageSender<ReportGenerationMessage>(sendEndpointProvider)
    {

        protected override string ConfigPathOrQueueName => "QueueReportGeneration";
    }
}