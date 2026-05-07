using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Services;

namespace ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;

public class WorkspaceDefinitionHandler : IRequestHandler<WorkspaceDefinition, PullRequestUpdateMessage>
{
    private readonly ILogger<WorkspaceDefinitionHandler> _logger;
    private readonly IRepositoryService _repositoryService;

    public WorkspaceDefinitionHandler(ILogger<WorkspaceDefinitionHandler> logger,
        IRepositoryService repositoryService)
    {
        _logger = logger;
        _repositoryService = repositoryService;
    }

    public async Task<PullRequestUpdateMessage> Handle(WorkspaceDefinition request,
        CancellationToken cancellationToken)
    {
        var pullRequestMessage = await _repositoryService.HandleResourcing(request);

        _logger.LogInformation("Pull request created for {WorkspaceAcronym}", request.Workspace.Acronym);
        return await Task.FromResult(pullRequestMessage);
    }
}
