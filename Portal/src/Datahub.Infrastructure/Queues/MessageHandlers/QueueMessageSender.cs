using Azure.Storage.Queues;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Datahub.Infrastructure.Queues.MessageHandlers;

public abstract class QueueMessageSender<T> : IRequestHandler<T> where T : IRequest
{
    protected readonly IConfiguration _configuration;

    public QueueMessageSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Handle(T request, CancellationToken cancellationToken)
    {
        var storageConnectionString = _configuration["DatahubStorageConnectionString"] ?? _configuration["DatahubStorageQueue:ConnectionString"];
        var queueName = _configuration[ConfigPathOrQueueName] ?? ConfigPathOrQueueName;

        if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(queueName)) 
        {
            throw new Exception("Invalid storage connection or queue configuration!");
        }

        var queueClient = new QueueClient(storageConnectionString, queueName);
        var message = EncodeBase64(JsonSerializer.Serialize(request));

        // pick up the timeout span
        TimeSpan? timeoutSpan = request is IMessageTimeout t ? TimeSpan.FromSeconds(t.Timeout) : default;

        await queueClient.SendMessageAsync(message, visibilityTimeout: timeoutSpan, cancellationToken: cancellationToken);
    }

    protected abstract string ConfigPathOrQueueName { get; }

    static string EncodeBase64(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

}

public interface IMessageTimeout
{
    public int Timeout { get; }
}
