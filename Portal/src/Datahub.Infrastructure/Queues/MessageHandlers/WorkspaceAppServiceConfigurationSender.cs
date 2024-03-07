using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class WorkspaceAppServiceConfigurationSender : QueueMessageSender<WorkspaceAppServiceConfigurationMessage>
    {
        public WorkspaceAppServiceConfigurationSender(IConfiguration configuration) : base(configuration)
        {
        }

        protected override string ConfigPathOrQueueName => "workspace-app-service-configuration";
    }
}