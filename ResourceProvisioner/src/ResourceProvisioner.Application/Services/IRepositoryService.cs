using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;

namespace ResourceProvisioner.Application.Services;

public interface IRepositoryService
{
    public Task FetchModuleRepository(string version);
    public Task FetchInfrastructureRepository();
    public Task CheckoutInfrastructureBranch(string workspaceName);
    public Task CommitTerraformTemplate(TerraformTemplate template, string username);
    public Task PushInfrastructureRepository(string workspaceAcronym);
    public Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcrynom);
    public Task FetchRepositoriesAndCheckoutProjectBranch(TerraformWorkspace workspace);
    public Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(WorkspaceDefinition command, string username);
    public Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate resourceTemplate, WorkspaceDefinition command, string username);

    public Task<PullRequestUpdateMessage> HandleResourcing(WorkspaceDefinition command);    
}