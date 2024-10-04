using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
    public class UserInactivityNotificationSender(ISendEndpointProvider sendEndpointProvider)
        : QueueMessageSender<UserInactivityNotificationMessage>(sendEndpointProvider)
    {

        protected override string ConfigPathOrQueueName =>
            QueueConstants.UserInactivityNotification;
    }
}