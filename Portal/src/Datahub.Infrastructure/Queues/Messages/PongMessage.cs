using MediatR;
namespace Datahub.Infrastructure.Queues.Messages;
public record PongMessage(string Pong) : IRequest;
