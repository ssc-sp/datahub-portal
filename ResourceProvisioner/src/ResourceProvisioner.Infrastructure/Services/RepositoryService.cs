using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Enums;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    private readonly ILogger<RepositoryService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITerraformService _terraformService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SemaphoreSlim _semaphore;

    public RepositoryService(IHttpClientFactory httpClientFactory, ILogger<RepositoryService> logger, IConfiguration configuration,
        ITerraformService terraformService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
        _terraformService = terraformService;

        _semaphore = new SemaphoreSlim(1, 1);
    }
    
    public async Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command)
    {
        await _semaphore.WaitAsync();
        
        var user = command.RequestingUserEmail ?? throw new NullReferenceException("Requesting user's email is null");
        _logger.LogInformation("Checking out workspace branch for {WorkspaceAcronym}", command.Workspace.Acronym);
        await FetchRepositoriesAndCheckoutProjectBranch(command.Workspace.Acronym);

        _logger.LogInformation("Executing resource runs for user {User}", user);
        var repositoryUpdateEvents =
            await ExecuteResourceRuns(command.Templates, command.Workspace, user);

        _logger.LogInformation("Pushing changes to remote repository for {WorkspaceAcronym}",
            command.Workspace.Acronym);
        await PushInfrastructureRepository(command.Workspace.Acronym);

        _logger.LogInformation("Creating pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
        var pullRequestValueObject =
            await CreateInfrastructurePullRequest(command.Workspace.Acronym, user);

        var pullRequestMessage = new PullRequestUpdateMessage
        {
            PullRequestValueObject = pullRequestValueObject,
            TerraformWorkspace = command.Workspace,
            Events = repositoryUpdateEvents
        };

        _semaphore.Release();
        return pullRequestMessage;
    }

    public Task FetchModuleRepository()
    {
        var repositoryUrl = _configuration["ModuleRepository:Url"];
        var localPath = _configuration["ModuleRepository:LocalPath"];

        _logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetModuleRepositoryPath(_configuration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
        Repository.Clone(repositoryUrl, repositoryPath);

        _logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
        return Task.CompletedTask;
    }

    public Task FetchInfrastructureRepository()
    {
        var localPath = _configuration["InfrastructureRepository:LocalPath"];
        var repositoryUrl = _configuration["InfrastructureRepository:Url"];
        _logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        var co = new CloneOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
            {
                Username = _configuration["InfrastructureRepository:Username"],
                Password = _configuration["InfrastructureRepository:Password"]
            }
        };

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
        Repository.Clone(repositoryUrl, repositoryPath, co);

        _logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
        return Task.CompletedTask;
    }

    public Task CheckoutInfrastructureBranch(string workspaceName)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);
        _logger.LogInformation("Checking out branch {WorkspaceName} in {Path}", workspaceName, repositoryPath);
        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[workspaceName];
        if (branch == null)
        {
            _logger.LogInformation("Branch {WorkspaceName} does not exist in {Path}, creating it now", workspaceName,
                repositoryPath);
            branch = repo.CreateBranch(workspaceName);
        }

        Commands.Checkout(repo, branch);

        _logger.LogInformation("Branch {WorkspaceName} checked out in {Path}", workspaceName, repositoryPath);

        _logger.LogInformation("Checking upstream for any updates in branch");

        var pullOptions = new PullOptions()
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
                {
                    Username = _configuration["InfrastructureRepository:Username"],
                    Password = _configuration["InfrastructureRepository:Password"]
                }
            }
        };

        var signature = new Signature(new Identity("Auto-merge", "Auto-merge"), DateTimeOffset.Now);
        try
        {
            var remote = repo.Network.Remotes["origin"];
            repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);
            Commands.Pull(repo, signature, pullOptions);
        }
        catch (MergeFetchHeadNotFoundException)
        {
            _logger.LogInformation("No upstream updates found");
        }

        return Task.CompletedTask;
    }

    public Task CommitTerraformTemplate(TerraformTemplate template, string username)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);

        _logger.LogInformation("Committing changes in {LocalPath}", repositoryPath);
        using var repository = new Repository(repositoryPath);

        _logger.LogInformation("Adding all files in {LocalPath}", repositoryPath);
        Commands.Stage(repository, "*");

        var author = new Signature(username, username, DateTimeOffset.Now);
        _logger.LogInformation(
            "Committing all files in {LocalPath} for module [{ModuleVersion}]{ModuleName} as {Author}", repositoryPath,
            template.Version, template.Name, author);
        try
        {
            repository.Commit($"Committing [{template.Version}]{template.Name} changes", author, author);
            _logger.LogInformation("Changes committed in {LocalPath}", repositoryPath);
        }
        catch (EmptyCommitException e)
        {
            _logger.LogInformation(e, "No changes to commit in {LocalPath}", repositoryPath);
            throw new NoChangesDetectedException($"No changes detected after adding {template.Name} to project");
        }

        return Task.CompletedTask;
    }

    public async Task PushInfrastructureRepository(string workspaceAcronym)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);
        var options = new PushOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
            {
                Username = _configuration["InfrastructureRepository:Username"],
                Password = _configuration["InfrastructureRepository:Password"]
            },
        };

        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[workspaceAcronym];
        var remote = repo.Network.Remotes["origin"];
        repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);

        _logger.LogInformation("Pushing changes in {LocalPath} to {Branch} branch", repositoryPath,
            branch.CanonicalName);

        await Task.Run(() => repo.Network.Push(repo.Branches[workspaceAcronym], options));

        _logger.LogInformation("Changes pushed in {LocalPath} to {Branch} branch", repositoryPath,
            branch.CanonicalName);
    }

    public async Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcronym, string username)
    {
        // create a pull request in Azure DevOps
        _logger.LogInformation("Creating infrastructure pull request");
        var postBody = BuildPullRequestPostBody(workspaceAcronym);

        var postUrl =
            $"{_configuration["InfrastructureRepository:PullRequestUrl"]}?api-version={_configuration["InfrastructureRepository:ApiVersion"]}";

        _logger.LogInformation("Posting infrastructure pull request to {Url}", postUrl);
        using var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.PostAsync(postUrl, postBody);

        // get the pull request id
        _logger.LogInformation("Getting infrastructure pull request url");
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonNode>(content);

        var pullRequestId =
            data?["pullRequestId"]?.ToString();
        
        // TODO: Test this!
        if (string.IsNullOrWhiteSpace(pullRequestId))
        {
            if (data?["typeKey"]?.ToString() == "GitPullRequestExistsException")
            {
                pullRequestId = await GetExistingPullRequestId(workspaceAcronym);
            }
            else
            {
                throw new Exception($"Could not get pull request id for {workspaceAcronym}");
            }
        }
        
        var pullRequestUrl = BuildPullRequestUrl(pullRequestId!);
        _logger.LogInformation("Infrastructure pull request url is {PullRequestUrl}", pullRequestUrl);

        return new PullRequestValueObject(workspaceAcronym, pullRequestUrl);
    }

    private string BuildPullRequestUrl(string pullRequestId)
    {
        return $"{_configuration["InfrastructureRepository:PullRequestBrowserUrl"]}/{pullRequestId}";
    }

    private StringContent BuildPullRequestPostBody(string workspaceAcronym)
    {
        var postData = new JsonObject
        {
            ["sourceRefName"] = $"refs/heads/{workspaceAcronym}",
            ["targetRefName"] = $"refs/heads/{_configuration["InfrastructureRepository:MainBranch"]}",
            ["title"] = $"[{workspaceAcronym}] Infrastructure changes",
            ["description"] = $"[{workspaceAcronym}] Infrastructure changes",
        };
        var postBody = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");
        return postBody;
    }

    public async Task FetchRepositoriesAndCheckoutProjectBranch(string workspaceAcronym)
    {
        await FetchModuleRepository();
        await FetchInfrastructureRepository();
        await CheckoutInfrastructureBranch(workspaceAcronym);
    }

    public async Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(List<TerraformTemplate> modules, TerraformWorkspace terraformWorkspace, string requestingUsername)
    {
        var repositoryUpdateEvents = new List<RepositoryUpdateEvent>();

        foreach (var module in modules)
        {
            var result = await ExecuteResourceRun(module, terraformWorkspace, requestingUsername);
            repositoryUpdateEvents.Add(result);
        }

        return repositoryUpdateEvents;
    }

    public async Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate template, TerraformWorkspace terraformWorkspace,
        string requestingUsername)
    {
        try
        {
            await _terraformService.CopyTemplateAsync(template, terraformWorkspace);
            await _terraformService.ExtractVariables(template, terraformWorkspace);
            if (template.Name == TerraformTemplate.NewProjectTemplate)
            {
                await _terraformService.ExtractBackendConfig(terraformWorkspace.Acronym);
            }
            await CommitTerraformTemplate(template, requestingUsername);

            return new RepositoryUpdateEvent()
            {
                Message =
                    $"Successfully created resource run for [{template.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.Success
            };
        }
        catch (NoChangesDetectedException)
        {
            return new RepositoryUpdateEvent()
            {
                Message = $"No changes detected after resource run for [{template.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.NoChangesDetected
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error while creating resource run for [{ModuleVersion}]{ModuleName} in {WorkspaceAcronym}",
                template.Version, template.Name, terraformWorkspace.Acronym);

            return new RepositoryUpdateEvent()
            {
                Message = $"Error creating resource run for [{template.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.Error
            };
        }
    }


    private async Task<string> GetExistingPullRequestId(string workspaceAcronym)
    {
        _logger.LogInformation("Pull request already exists, fetching pull request id");
        var url =
            $"{_configuration["InfrastructureRepository:PullRequestUrl"]}?searchCriteria.status=active&searchCriteria.sourceRefName=refs/heads/{workspaceAcronym}&api-version={_configuration["InfrastructureRepository:ApiVersion"]}";
        
        using var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonNode>(content);
        
        return data?["value"]?
            .AsArray()
            .FirstOrDefault(node => node?["sourceRefName"]?.ToString() == $"refs/heads/{workspaceAcronym}")?
            .AsObject()["pullRequestId"]?.ToString() ?? throw new NullReferenceException($"Could not get existing pull request id for workspace {workspaceAcronym}");
    }
}