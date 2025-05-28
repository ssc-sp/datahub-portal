using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class VersionUpdateSender(ISendEndpointProvider sendEndpointProvider)
        : QueueMessageSender<VersionUpdateMessage>(sendEndpointProvider)
    {

        protected override string ConfigPathOrQueueName =>
            QueueConstants.WorkspaceVersionUpdateRequestQueueName;
    }
}
