using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers
{
	public class UserInactivityNotificationSender : QueueMessageSender<UserInactivityNotificationMessage>
	{
		public UserInactivityNotificationSender(IConfiguration configuration) : base(configuration)
		{
		}

		protected override string ConfigPathOrQueueName => "QueueUserInactivityNotification";
	}
}