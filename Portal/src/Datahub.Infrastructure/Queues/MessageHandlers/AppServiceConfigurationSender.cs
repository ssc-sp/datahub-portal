using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class AppServiceConfigurationSender : QueueMessageSender<AppServiceConfigurationMessage>
    {
        public AppServiceConfigurationSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "app-service-configuration";
    }
}