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
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Common;
using Version = System.Version;

namespace ResourceProvisioner.Infrastructure.Services;

public class RepositoryService : IRepositoryService
{
    private readonly ILogger<RepositoryService> _logger;
    private readonly ITerraformService _terraformService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SemaphoreSlim _semaphore;
    private readonly ResourceProvisionerConfiguration _resourceProvisionerConfiguration;

    public RepositoryService(IHttpClientFactory httpClientFactory, ILogger<RepositoryService> logger, ResourceProvisionerConfiguration resourceProvisionerConfiguration,
        ITerraformService terraformService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _resourceProvisionerConfiguration = resourceProvisionerConfiguration;
        _terraformService = terraformService;

        _semaphore = new SemaphoreSlim(1, 1);
    }
    
    public async Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command)
    {
        await _semaphore.WaitAsync();
        
        var user = command.RequestingUserEmail ?? throw new NullReferenceException("Requesting user's email is null");
        _logger.LogInformation("Checking out workspace branch for {WorkspaceAcronym}", command.Workspace.Acronym);
        await FetchRepositoriesAndCheckoutProjectBranch(command.Workspace.Acronym!);

        _logger.LogInformation("Executing the following resource runs in workspace {WorkspaceAcronym} for user {User}: [{ResourceRuns}]",
            command.Workspace.Acronym, user, string.Join(", ", command.Templates.Select(x => x.Name)));
        var repositoryUpdateEvents =
            await ExecuteResourceRuns(command.Templates, command.Workspace, user);

        _logger.LogInformation("Pushing changes to remote repository for {WorkspaceAcronym}",
            command.Workspace.Acronym);
        await PushInfrastructureRepository(command.Workspace.Acronym!);

        _logger.LogInformation("Creating pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
        var pullRequestValueObject =
            await CreateInfrastructurePullRequest(command.Workspace.Acronym!, user);

                        
        if(!string.IsNullOrEmpty(_resourceProvisionerConfiguration.InfrastructureRepository.AutoApproveUserOid))
        {
            _logger.LogInformation("Auto-approving pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
            await AutoApproveInfrastructurePullRequest(pullRequestValueObject.PullRequestId);
        }
        else
        {
            _logger.LogInformation("Auto-approve user OID not set, skipping auto-approve");
        }
        
        var pullRequestMessage = new PullRequestUpdateMessage
        {
            PullRequestValueObject = pullRequestValueObject,
            TerraformWorkspace = command.Workspace,
            Events = repositoryUpdateEvents
        };

        _semaphore.Release();
        return pullRequestMessage;
    }

    public async Task<List<Version>> GetModuleVersions()
    {
        var repositoryPath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        var modulePath = Path.Combine(repositoryPath, _resourceProvisionerConfiguration.ModuleRepository.ModulePathPrefix);
        
        // check if module path exists
        if(!Directory.Exists(modulePath))
        {
            _logger.LogInformation("Module path {ModulePath} does not exist, fetching module repository", modulePath);
            await FetchModuleRepository();
        }
        
        var versions = Directory.GetDirectories(modulePath)
            .Select(x => 
                new Version(x
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Split(Path.DirectorySeparatorChar)
                    .Last()[1..] // remove the v prefix
                ))
            .ToList();
        return versions;
    }

    public Task FetchModuleRepository()
    {
        var repositoryUrl = _resourceProvisionerConfiguration.ModuleRepository.Url;
        var localPath = _resourceProvisionerConfiguration.ModuleRepository.LocalPath;

        _logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
        Repository.Clone(repositoryUrl, repositoryPath);

        if (_resourceProvisionerConfiguration.ModuleRepository.Branch != ModuleRepositoryConfiguration.DefaultBranch)
        {
            using var repo = new Repository(repositoryPath);
            var branch = repo.Branches[$"refs/remotes/origin/{_resourceProvisionerConfiguration.ModuleRepository.Branch}"];
            if (branch == null)
            {
                _logger.LogInformation("Branch {Branch} does not exist, checking out default branch", _resourceProvisionerConfiguration.ModuleRepository.Branch);
                branch = repo.Branches[ModuleRepositoryConfiguration.DefaultBranch];
            }
            Commands.Checkout(repo, branch);
        }

        _logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
        return Task.CompletedTask;
    }

    public Task FetchInfrastructureRepository()
    {
        var localPath = _resourceProvisionerConfiguration.InfrastructureRepository.LocalPath;
        var repositoryUrl = _resourceProvisionerConfiguration.InfrastructureRepository.Url;
        _logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        var co = new CloneOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
            {
                Username = _resourceProvisionerConfiguration.InfrastructureRepository.Username,
                Password = _resourceProvisionerConfiguration.InfrastructureRepository.Password
            }
        };

        _logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
        Repository.Clone(repositoryUrl, repositoryPath, co);

        _logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
        return Task.CompletedTask;
    }

    public Task CheckoutInfrastructureBranch(string workspaceName)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
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
                    Username = _resourceProvisionerConfiguration.InfrastructureRepository.Username,
                    Password = _resourceProvisionerConfiguration.InfrastructureRepository.Password
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
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);

        _logger.LogInformation("Committing changes in {LocalPath}", repositoryPath);
        using var repository = new Repository(repositoryPath);

        _logger.LogInformation("Adding all files in {LocalPath}", repositoryPath);
        Commands.Stage(repository, "*");

        var author = new Signature(username, username, DateTimeOffset.Now);
        _logger.LogInformation(
            "Committing all files in {LocalPath} for module {ModuleName} as {Author}", repositoryPath,
            template.Name, author);
        try
        {
            repository.Commit($"Committing {template.Name} changes", author, author);
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
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
        var options = new PushOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
            {
                Username = _resourceProvisionerConfiguration.InfrastructureRepository.Username,
                Password = _resourceProvisionerConfiguration.InfrastructureRepository.Password
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
            $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}?api-version={_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        _logger.LogInformation("Posting infrastructure pull request to {Url}", postUrl);
        var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
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
        
        var pullRequestUrl = BuildPullRequestUrl(pullRequestId);
        _logger.LogInformation("Infrastructure pull request url is {PullRequestUrl}", pullRequestUrl);

        return new PullRequestValueObject(workspaceAcronym, pullRequestUrl, int.Parse(pullRequestId));
    }

    private async Task AutoApproveInfrastructurePullRequest(int pullRequestId)
    {
        var patchContent = BuildPullRequestPatchBody();
        var patchUrl =
            $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}/{pullRequestId}?api-version={_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        _logger.LogInformation("Patching auto-approve infrastructure pull request to {Url}", patchUrl);
        var httpClient = _httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.PatchAsync(patchUrl, patchContent);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Could not auto-approve infrastructure pull request {PullRequestUrl}", patchUrl);
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error: {Error}", content);
        }
        else
        {
            _logger.LogInformation("Infrastructure pull request {PullRequestUrl} auto-approved", patchUrl);
        }
        
    }

    private StringContent BuildPullRequestPatchBody()
    {
        _logger.LogInformation("Building infrastructure pull request patch body for auto-approve user {AutoApproveUserOid}", _resourceProvisionerConfiguration.InfrastructureRepository.AutoApproveUserOid);
        var patchData = new JsonObject
        {
            ["autoCompleteSetBy"] = new JsonObject
            {
                ["id"] = _resourceProvisionerConfiguration.InfrastructureRepository.AutoApproveUserOid
            }
        };
        var patchBody = new StringContent(JsonSerializer.Serialize(patchData), Encoding.UTF8, "application/json");
        return patchBody;
    }
    
    private string BuildPullRequestUrl(string pullRequestId)
    {
        return $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestBrowserUrl}/{pullRequestId}";
    }

    private StringContent BuildPullRequestPostBody(string workspaceAcronym)
    {
        var postData = new JsonObject
        {
            ["sourceRefName"] = $"refs/heads/{workspaceAcronym}",
            ["targetRefName"] = $"refs/heads/{_resourceProvisionerConfiguration.InfrastructureRepository.MainBranch}",
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

        await ValidateWorkspaceVersion(terraformWorkspace);

        foreach (var module in modules)
        {
            var result = await ExecuteResourceRun(module, terraformWorkspace, requestingUsername);
            repositoryUpdateEvents.Add(result);
        }

        return repositoryUpdateEvents;
    }

    public async Task ValidateWorkspaceVersion(TerraformWorkspace terraformWorkspace)
    {
        // Old behavior, to maintain existing versions
        // if (terraformWorkspace.Version == TerraformWorkspace.DefaultVersion)
        // {
        //     var versions = await GetModuleVersions();
        //     var latestVersion = versions.Max();
        //     terraformWorkspace.Version = $"v{latestVersion!.ToString()}";
        // }
        
        // new behavior to always update the version
        var versions = await GetModuleVersions();
        var latestVersion = versions.Max();
        terraformWorkspace.Version = $"v{latestVersion!.ToString()}";
    }

    public async Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate template, TerraformWorkspace terraformWorkspace,
        string requestingUsername)
    {
        try
        {
            if (template.Status == "delete") { await DeleteTemplateAsync(template, terraformWorkspace); }
            else {
                await _terraformService.CopyTemplateAsync(template, terraformWorkspace);
                await _terraformService.ExtractVariables(template, terraformWorkspace);
                switch (template.Name) {
                    case TerraformTemplate.NewProjectTemplate:
                        // Constructs the project.tfbackend file for the given acronym of the project 
                        // from the configuration and the workspace acronym using static logic
                        await _terraformService.ExtractBackendConfig(terraformWorkspace.Acronym!);
                        break;
                    case TerraformTemplate.VariableUpdate:
                        //for each template of the workspace, reconstructs all the variable values required
                        //by the template and writes it into the variables file 
                        await _terraformService.ExtractAllVariables(terraformWorkspace);
                        break;
                }
            }
            await CommitTerraformTemplate(template, requestingUsername);
            return new RepositoryUpdateEvent() {
                Message =
                    $"Successfully created resource run for [{terraformWorkspace.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.Success
            };
        }
        catch (NoChangesDetectedException)
        {
            return new RepositoryUpdateEvent()
            {
                Message = $"No changes detected after resource run for [{terraformWorkspace.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.NoChangesDetected
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error while creating resource run for [{ModuleVersion}]{ModuleName} in {WorkspaceAcronym}",
                terraformWorkspace.Version, template.Name, terraformWorkspace.Acronym);

            return new RepositoryUpdateEvent()
            {
                Message = $"Error creating resource run for [{terraformWorkspace.Version}]{template.Name} in {terraformWorkspace.Acronym}",
                StatusCode = MessageStatusCode.Error
            };
        }
    }

    private async Task DeleteTemplateAsync(TerraformTemplate template, TerraformWorkspace terraformWorkspace)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var searchPattern = template.Name + "*.*"; // Pattern to match <templateName>*.*
        var matchingFiles = Directory.GetFiles(projectPath, searchPattern);

        if (matchingFiles.Length > 0)
        {
            foreach (var filePath in matchingFiles) {
                File.Delete(filePath);
                _logger.LogInformation($"Deleted file: {filePath}");
            }

            // Once all files are deleted, create a new .tf file with output indicating deletion
            var deletionIndicatorFilePath = Path.Combine(projectPath, template.Name + ".tf");
            var deletionIndicatorContent = @"output ""azure_storage_blob_status"" {
  value = ""deleted""
}";
            await File.WriteAllTextAsync(deletionIndicatorFilePath, deletionIndicatorContent);
            _logger.LogInformation($"Deletion indicator added for {template.Name} in {projectPath}.");
        }
        else
        {
            _logger.LogWarning($"No matching files found for template {template.Name} in {projectPath}.");
        }
    }
    private async Task<string> GetExistingPullRequestId(string workspaceAcronym)
    {
        _logger.LogInformation("Pull request already exists, fetching pull request id");
        var url =
            $"{_resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}?searchCriteria.status=active&searchCriteria.sourceRefName=refs/heads/{workspaceAcronym}&api-version={_resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";
        
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