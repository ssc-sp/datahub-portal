using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record WorkspaceAppServiceConfigurationMessage(
        string ProjectAcronym,
        AppServiceConfiguration Configuration
        ) : IRequest;
}