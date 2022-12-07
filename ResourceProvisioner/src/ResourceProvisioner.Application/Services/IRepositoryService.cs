using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Domain.Entities;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;

namespace ResourceProvisioner.Application.Services;

public interface IRepositoryService
{
    public Task FetchModuleRepository();
    public Task FetchInfrastructureRepository();
    public Task CheckoutInfrastructureBranch(string workspaceName);
    public Task CommitDataHubTemplate(DataHubTemplate template, string username);
    public Task PushInfrastructureRepository(string workspaceAcronym);
    public Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcrynom, string username);
    public Task FetchRepositoriesAndCheckoutProjectBranch(string workspaceAcronym);
    public Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(List<DataHubTemplate> modules, string workspaceAcronym, string requestingUsername);
    public Task<RepositoryUpdateEvent> ExecuteResourceRun(DataHubTemplate templates, string workspaceAcronym, string requestingUsername);

    public Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command);
}