using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Shared.Messaging;
using MassTransit; 


namespace Datahub.Infrastructure.Services;

public class QueueMessageConsumer: IConsumer<IQueueMessage>
{
    protected readonly IConfiguration _configuration;
    public QueueMessageConsumer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<IQueueMessage> context)
    {
        //await _mediator.Send(context.Message);

        var ConfigPathOrQueueName = context is IQueueMessage m ? m.ConfigPathOrQueueName : null;
        var storageConnectionString = _configuration["DatahubStorageConnectionString"] ?? _configuration["DatahubStorageQueue:ConnectionString"];
        var queueName = _configuration[ConfigPathOrQueueName] ?? ConfigPathOrQueueName;

        if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(queueName))
        {
            throw new Exception("Invalid storage connection or queue configuration!");
        }

        var queueClient = new QueueClient(storageConnectionString, queueName);
        var message = context.Message.Content;

        await queueClient.CreateIfNotExistsAsync();

        // pick up the timeout span
        TimeSpan? timeoutSpan = context is IMessageTimeout t ? TimeSpan.FromSeconds(t.Timeout) : default;

        await queueClient.SendMessageAsync(message, visibilityTimeout: timeoutSpan);
    }
}

