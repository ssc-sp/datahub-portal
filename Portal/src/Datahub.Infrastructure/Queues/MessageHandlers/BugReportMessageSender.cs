using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class BugReportMessageSender : QueueMessageSender<BugReportMessage>
    {
        public BugReportMessageSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "bug-report";
    }
}