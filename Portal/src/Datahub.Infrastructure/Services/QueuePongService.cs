using Datahub.Infrastructure.Queues.Messages;
using MediatR;

namespace Datahub.Infrastructure.Services;

public class QueuePongService
{
    private readonly IMediator _mediator;

    public QueuePongService(IMediator mediator)
    {
        _mediator = mediator;
    }

    const string PING = "PING:";

    public async Task<bool> Pong(string message)
    {
        var isPing = (message ?? "").StartsWith(PING, StringComparison.OrdinalIgnoreCase);
        if (isPing)
        {
            await _mediator.Send(new PongMessage(message![PING.Length..].Trim()));
        }
        return isPing;
    }
}
