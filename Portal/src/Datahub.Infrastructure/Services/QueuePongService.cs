using Datahub.Infrastructure.Queues.Messages;
using MassTransit;

namespace Datahub.Infrastructure.Services;

public class QueuePongService
{
    private readonly IPublishEndpoint _publishEndpoint;

    public QueuePongService(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    const string PING = "PING:";

    public async Task<bool> Pong(string message)
    {
        var isPing = (message ?? "").StartsWith(PING, StringComparison.OrdinalIgnoreCase);
        if (isPing)
        {
            await _publishEndpoint.Publish(new PongMessage(message![PING.Length..].Trim()));
        }
        return isPing;
    }
}
