using MediatR;

namespace ResourceProvisioner.Infrastructure.Common;
public record PongMessage(string Pong) : IRequest;
