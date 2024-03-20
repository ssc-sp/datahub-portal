using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class EmailMessageSender : QueueMessageSender<EmailRequestMessage>
{
    public EmailMessageSender(IConfiguration configuration) : base(configuration)
    {
    }

    protected override string ConfigPathOrQueueName => "email-notification";
}
