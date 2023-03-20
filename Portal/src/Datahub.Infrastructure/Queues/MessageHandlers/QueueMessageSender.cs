using Azure.Storage.Queues;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public abstract class QueueMessageSender<T> : AsyncRequestHandler<T> where T : IRequest
{
    protected readonly IConfiguration _configuration;

    public QueueMessageSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task Handle(T request, CancellationToken cancellationToken)
    {
        var storageConnectionString = _configuration["DatahubStorageConnectionString"];
        var queueName = _configuration[ConfigPathOrQueueName] ?? ConfigPathOrQueueName;

        if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(queueName)) 
        {
            throw new Exception("Invalid storage connection or queue configuration!");
        }

        var queueClient = new QueueClient(storageConnectionString, queueName);
        var message = EncodeBase64(JsonSerializer.Serialize(request));

        await queueClient.SendMessageAsync(message, cancellationToken);
    }

    protected abstract string ConfigPathOrQueueName { get; }

    static string EncodeBase64(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
}
