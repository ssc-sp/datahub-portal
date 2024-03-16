using Datahub.Shared.Entities;
using Datahub.Shared.Messaging;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class CreateResourceRunCommand : IRequest<PullRequestUpdateMessage>
{
    public List<TerraformTemplate> Templates { get; set; }
    public TerraformWorkspace Workspace { get; set; }

    public WorkspaceAppData AppData { get; set; }

    public string RequestingUserEmail { get; set; }
}