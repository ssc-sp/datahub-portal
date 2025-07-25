using Datahub.Shared.Entities;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record InfrastructureHealthCheckResultMessage(
        string Group,
        string Name,
        InfrastructureHealthResourceType ResourceType,
        InfrastructureHealthStatus Status,
        DateTime HealthCheckTimeUtc,
        string Details
    ) : IRequest;
}