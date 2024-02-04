using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record AppServiceConfigurationMessage(
        string ProjectAcronym
        ) : IRequest;
}