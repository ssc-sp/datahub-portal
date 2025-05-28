using MediatR;
using ResourceProvisioner.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResourceProvisioner.Application.WorkspaceVersion.Commands.WorkspaceVersionUpdate
{
    public class WorkspaceVersionUpdateCommand : IRequest<PullRequestUpdateMessage>
    {
        public List<String> ProjectIds { get; set; }
        
    }
}
