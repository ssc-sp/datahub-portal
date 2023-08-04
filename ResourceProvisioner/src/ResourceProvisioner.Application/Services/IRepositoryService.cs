using Datahub.Shared.Entities;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;

namespace ResourceProvisioner.Application.Services;

public interface IRepositoryService
{
    public Task FetchModuleRepository();
    public Task FetchInfrastructureRepository();
    public Task CheckoutInfrastructureBranch(string workspaceName);
    public Task CommitTerraformTemplate(TerraformTemplate template, string username);
    public Task PushInfrastructureRepository(string workspaceAcronym);
    public Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcrynom, string username);
    public Task FetchRepositoriesAndCheckoutProjectBranch(string workspaceAcronym);
    public Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(List<TerraformTemplate> modules, TerraformWorkspace terraformWorkspace, string requestingUsername);
    public Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate templates, TerraformWorkspace terraformWorkspace, string requestingUsername);

    public Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command);
    public Task<List<Version>> GetModuleVersions();
}