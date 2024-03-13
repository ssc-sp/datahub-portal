using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record WorkspaceAppServiceConfigurationMessage(
        string ProjectAcronym
        ) : IRequest;
}