using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class InfrastructureHealthCheckSender : QueueMessageSender<InfrastructureHealthCheckMessage>
    {
        public InfrastructureHealthCheckSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "infrastructure-health-check";
    }
}