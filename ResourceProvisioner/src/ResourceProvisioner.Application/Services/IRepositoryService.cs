using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;

namespace ResourceProvisioner.Application.Services;

public interface IRepositoryService
{
    public void FetchModuleRepository();
    public Task FetchInfrastructureRepository();
    public Task CheckoutInfrastructureBranch(string workspaceName);
    public Task CommitTerraformTemplate(TerraformTemplate template);
    public Task PushInfrastructureRepository(string workspaceAcronym);
    public Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcrynom);
    public Task FetchRepositoriesAndCheckoutProjectBranch(string workspaceAcronym);
    public Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(CreateResourceRunCommand command);
    public Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate resourceTemplate, CreateResourceRunCommand command);

    public Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command);
    public Task<List<Version>> GetModuleVersions();
    
    /// <summary>
    /// Takes a workspace and validates that the version is valid for the workspace. This will update the workspace version if it is marked as latest.
    /// </summary>
    /// <param name="terraformWorkspace"></param>
    /// <returns></returns>
    public Task ValidateWorkspaceVersion(TerraformWorkspace terraformWorkspace);
}