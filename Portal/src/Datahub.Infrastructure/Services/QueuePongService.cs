using Datahub.Application.Services;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;

namespace Datahub.Infrastructure.Services;

public class QueuePongService(ISendEndpointProvider sendEndpointProvider): IQueuePongService
{
    private const string Ping = "PING:";

    public virtual async Task<bool> Pong(string message)
    {
        var isPing = (message ?? "").StartsWith(Ping, StringComparison.OrdinalIgnoreCase);
        if (isPing)
        {
            await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.PongQueueName, new PongMessage(message![Ping.Length..].Trim()));
        }
        return isPing;
    }
}
