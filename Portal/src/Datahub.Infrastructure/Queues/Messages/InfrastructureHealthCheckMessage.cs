using Datahub.Shared.Entities;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record InfrastructureHealthCheckMessage(
        InfrastructureHealthResourceType Type,
        string Group,
        string Name
    ) : IRequest;
}