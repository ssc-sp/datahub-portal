using Datahub.Shared.Entities;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record WorkspaceAppServiceConfigurationMessage(
        string ProjectAcronym,
        AppServiceConfiguration Configuration
        ) : IRequest;
}