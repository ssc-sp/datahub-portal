using MediatR;
using Datahub.Core.Model.Health;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record InfrastructureHealthCheckMessage(
        InfrastructureHealthResourceType Type,
        string Group,
        string Name
    ) : IRequest;
}