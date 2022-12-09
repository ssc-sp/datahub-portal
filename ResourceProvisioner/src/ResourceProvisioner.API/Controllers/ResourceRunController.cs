using Microsoft.AspNetCore.Mvc;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Domain.Messages;

namespace ResourceProvisioner.API.Controllers;

public class ResourceRunController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PullRequestUpdateMessage>> Create(CreateResourceRunCommand command)
    {
        return await Mediator.Send(command);
    }
}