using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class PongMessageHandler : QueueMessageSender<PongMessage>
{
	public PongMessageHandler(IConfiguration configuration) : base(configuration)
	{
	}

	protected override string ConfigPathOrQueueName => "pong-queue";
}
