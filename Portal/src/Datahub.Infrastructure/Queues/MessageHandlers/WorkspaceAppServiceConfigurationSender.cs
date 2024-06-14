using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class WorkspaceAppServiceConfigurationSender(ISendEndpointProvider sendEndpointProvider)
        : QueueMessageSender<WorkspaceAppServiceConfigurationMessage>(sendEndpointProvider)
    {

        protected override string ConfigPathOrQueueName => QueueConstants.WorkspaceAppServiceConfigurationQueueName;
    }
}