using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public class StorageAccountSender : QueueMessageSender<StorageAccountMessage>
{
    public StorageAccountSender(IConfiguration configuration) : base(configuration)
    {
    }

    protected override string ConfigPathOrQueueName => "storage-capacity";
}
