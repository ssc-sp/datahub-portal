using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Services;

namespace ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

public class CreateResourceRunCommand : IRequest<PullRequestUpdateMessage>
{
    public List<TerraformTemplate> Templates { get; set; }
    public TerraformWorkspace Workspace { get; set; }
    
    public WorkspaceAppData AppData { get; set; }

    public string RequestingUserEmail { get; set; }
}

public class CreateResourceRunCommandHandler : IRequestHandler<CreateResourceRunCommand, PullRequestUpdateMessage>
{
    private readonly ILogger<CreateResourceRunCommandHandler> _logger;
    private readonly IRepositoryService _repositoryService;

    public CreateResourceRunCommandHandler(ILogger<CreateResourceRunCommandHandler> logger,
        IRepositoryService repositoryService)
    {
        _logger = logger;
        _repositoryService = repositoryService;
    }

    public async Task<PullRequestUpdateMessage> Handle(CreateResourceRunCommand request,
        CancellationToken cancellationToken)
    {
        var pullRequestMessage = await _repositoryService.HandleResourcing(request);

        _logger.LogInformation("Pull request created for {WorkspaceAcronym}", request.Workspace.Acronym);
        return await Task.FromResult(pullRequestMessage);
    }
}