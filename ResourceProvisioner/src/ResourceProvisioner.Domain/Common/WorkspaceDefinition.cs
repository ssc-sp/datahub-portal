using MediatR;
using ResourceProvisioner.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Shared.Entities
{
    public partial class WorkspaceDefinition : IRequest<PullRequestUpdateMessage>
    {

    }
}
