using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class ReportGenerationMessageSender : QueueMessageSender<ReportGenerationMessage>
    {
        public ReportGenerationMessageSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "QueueReportGeneration";
    }
}