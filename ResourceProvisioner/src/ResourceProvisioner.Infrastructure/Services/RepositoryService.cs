using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Datahub.Shared;
using Datahub.Shared.Clients;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Enums;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Domain.Messages;
using ResourceProvisioner.Domain.ValueObjects;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Polly;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Common;
using Version = System.Version;

namespace ResourceProvisioner.Infrastructure.Services;

public partial class RepositoryService(
    IHttpClientFactory httpClientFactory,
    ILogger<RepositoryService> logger,
    ResourceProvisionerConfiguration resourceProvisionerConfiguration,
    ITerraformService terraformService)
    : IRepositoryService
{
    /// <summary>
    /// Retrieves the regular expression used for matching module versions in the module repository.
    /// The regular expression pattern matches the directory structure of the module repository versions in the format vX.Y.Z.
    /// </summary>
    /// <returns>The regular expression pattern for matching module versions.</returns>
    [GeneratedRegex(@"(/|\\)v\d+\.\d+\.\d+$")]
    private static partial Regex ModuleRegex();

    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly SemaphoreSlim _moduleSemaphore = new(1, 1);
    
    public async Task<PullRequestUpdateMessage> HandleResourcing(CreateResourceRunCommand command)
    {
        await _semaphore.WaitAsync();
        try
        {
            DirectoryUtils.tempDirectory = Guid.NewGuid().ToString().Substring(0, 8);
            CreateTemporaryDirectory();

            var user = command.RequestingUserEmail ??
                       throw new NullReferenceException("Requesting user's email is null");
            logger.LogInformation("Checking out workspace branch for {WorkspaceAcronym}", command.Workspace.Acronym);
            await FetchRepositoriesAndCheckoutProjectBranch(command.Workspace);

            logger.LogInformation(
                "Executing the following resource runs in workspace {WorkspaceAcronym} for user {User}: [{ResourceRuns}]",
                command.Workspace.Acronym, user, string.Join(", ", command.Templates.Select(x => x.Name)));
            var repositoryUpdateEvents =
                await ExecuteResourceRuns(command, user);

            logger.LogInformation("Pushing changes to remote repository for {WorkspaceAcronym}",
                command.Workspace.Acronym);
            await PushInfrastructureRepository(command.Workspace.Acronym!);

            logger.LogInformation("Creating pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
            var pullRequestValueObject =
                await CreateInfrastructurePullRequest(command.Workspace.Acronym!);

            logger.LogInformation("Completing pull request for {WorkspaceAcronym}", command.Workspace.Acronym);
            await AutoApproveInfrastructurePullRequest(pullRequestValueObject.PullRequestId,
                command.Workspace.Acronym!);


            var pullRequestMessage = new PullRequestUpdateMessage
            {
                PullRequestValueObject = pullRequestValueObject,
                TerraformWorkspace = command.Workspace,
                Events = repositoryUpdateEvents
            };

            if (pullRequestMessage.Events.All(x => x.StatusCode != MessageStatusCode.Error))
            {
                return pullRequestMessage;
            }

            pullRequestMessage.Events
                .Where(x => x.StatusCode == MessageStatusCode.Error)
                .ToList()
                .ForEach(x => logger.LogError(x.Message, x));
            throw new Exception("Error while handling resource run request");
        }
        finally
        {
            CleanUpEnvironment();
            _semaphore.Release();
        }
    }

    private void CreateTemporaryDirectory()
    {
        CleanUpEnvironment();
        var tempPath = DirectoryUtils.GetTempDirectoryPath(resourceProvisionerConfiguration);
        logger.LogInformation("Creating temporary directory {Directory} for resource run", Path.GetFullPath(tempPath));
        Directory.CreateDirectory(tempPath);
    }

    public async Task FetchModuleRepository(string version)
    {
        _moduleSemaphore.Wait();
        try
        {
            var repositoryUrl = resourceProvisionerConfiguration.ModuleRepository.Url;
            var localPath = resourceProvisionerConfiguration.ModuleRepository.LocalPath;
            var branch = resourceProvisionerConfiguration.ModuleRepository.Branch;
            version = $"{branch}-{version}";
            logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
            var repositoryPath = DirectoryUtils.GetModuleRepositoryPath(resourceProvisionerConfiguration);
            DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

            logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl, repositoryPath);
            Repository.Clone(repositoryUrl, repositoryPath);

            using var repo = new Repository(repositoryPath);
            var repoTag = string.IsNullOrWhiteSpace(version) ? null : repo.Tags[version];

            if (repoTag == null)
            {
                logger.LogInformation("Tag {BranchOrTag} does not exist, checking out default branch",
                    repoTag);
                var branchTag = repo.Branches[ModuleRepositoryConfiguration.DefaultBranch];
                Commands.Checkout(repo, branchTag);
            }
            else
            {
                Commands.Checkout(repo, repoTag.Target.Sha);
            }

            logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl, repositoryPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while fetching module repository");
            throw new Exception("Error while fetching module repository", e);
        }
        finally
        {
            _moduleSemaphore.Release();
        }
    }

    public async Task FetchInfrastructureRepository()
    {
        var localPath = resourceProvisionerConfiguration.InfrastructureRepository.LocalPath;
        var repositoryUrl = resourceProvisionerConfiguration.InfrastructureRepository.Url;
        logger.LogInformation("Fetching repository {RepositoryUrl} to {LocalPath}", repositoryUrl, localPath);
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(resourceProvisionerConfiguration);
        DirectoryUtils.VerifyDirectoryDoesNotExist(repositoryPath);

        var azureDevOpsClient =
            new AzureDevOpsClient(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
        var accessToken = await azureDevOpsClient.AccessTokenAsync();

        var cloneOptions = new CloneOptions
        {
            FetchOptions =
            {
                CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
                {
                    Username = resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration
                        .ClientId,
                    Password = accessToken.Token
                }
            }
        };

        logger.LogInformation("Cloning repository {RepositoryUrl} to {LocalPath}", repositoryUrl,
            Path.GetFullPath(repositoryPath));
        Repository.Clone(repositoryUrl, repositoryPath, cloneOptions);

        logger.LogInformation("Repository {RepositoryUrl} cloned to {LocalPath}", repositoryUrl,
            Path.GetFullPath(repositoryPath));
    }

    public async Task CheckoutInfrastructureBranch(string workspaceName)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(resourceProvisionerConfiguration);
        logger.LogInformation("Checking out branch {WorkspaceName} in {Path}", workspaceName, repositoryPath);
        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[workspaceName];
        if (branch == null)
        {
            logger.LogInformation("Branch {WorkspaceName} does not exist in {Path}, creating it now", workspaceName,
                repositoryPath);
            branch = repo.CreateBranch(workspaceName);
        }

        Commands.Checkout(repo, branch);

        logger.LogInformation("Branch {WorkspaceName} checked out in {Path}", workspaceName, repositoryPath);

        logger.LogInformation("Checking upstream for any updates in branch");

        var azureDevOpsClient =
            new AzureDevOpsClient(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
        var accessToken = await azureDevOpsClient.AccessTokenAsync();

        var pullOptions = new PullOptions()
        {
            FetchOptions = new FetchOptions
            {
                CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
                {
                    Username = resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration
                        .ClientId,
                    Password = accessToken.Token
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
            logger.LogInformation("No upstream updates found");
        }
    }

    public virtual Task CommitTerraformTemplate(TerraformTemplate template, string username)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(resourceProvisionerConfiguration);

        logger.LogInformation("Committing changes in {LocalPath}", repositoryPath);
        using var repository = new Repository(repositoryPath);

        logger.LogInformation("Adding all files in {LocalPath}", repositoryPath);
        Commands.Stage(repository, "*");

        var author = new Signature(username, username, DateTimeOffset.Now);
        logger.LogInformation(
            "Committing all files in {LocalPath} for module {ModuleName} as {Author}", repositoryPath,
            template.Name, author);
        try
        {
            repository.Commit($"Committing {template.Name} changes", author, author);
            logger.LogInformation("Changes committed in {LocalPath}", repositoryPath);
        }
        catch (EmptyCommitException e)
        {
            logger.LogInformation(e, "No changes to commit in {LocalPath}", repositoryPath);
            throw new NoChangesDetectedException($"No changes detected after adding {template.Name} to project");
        }

        return Task.CompletedTask;
    }

    public async Task PushInfrastructureRepository(string workspaceAcronym)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(resourceProvisionerConfiguration);

        var azureDevOpsClient =
            new AzureDevOpsClient(resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration);
        var accessToken = await azureDevOpsClient.AccessTokenAsync();
        var options = new PushOptions
        {
            CredentialsProvider = (_, _, _) => new UsernamePasswordCredentials()
            {
                Username = resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId,
                Password = accessToken.Token
            },
        };

        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[workspaceAcronym];
        var remote = repo.Network.Remotes["origin"];
        repo.Branches.Update(branch, b => b.Remote = remote.Name, b => b.UpstreamBranch = branch.CanonicalName);

        logger.LogInformation("Pushing changes in {LocalPath} to {Branch} branch", repositoryPath,
            branch.CanonicalName);

        await Task.Run(() => repo.Network.Push(repo.Branches[workspaceAcronym], options));

        logger.LogInformation("Changes pushed in {LocalPath} to {Branch} branch", repositoryPath,
            branch.CanonicalName);
    }

    public async Task<PullRequestValueObject> CreateInfrastructurePullRequest(string workspaceAcronym)
    {
        // create a pull request in Azure DevOps
        logger.LogInformation("Creating infrastructure pull request");
        var postBody = BuildPullRequestPostBody(workspaceAcronym);

        var postUrl =
            $"{resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}?api-version={resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        logger.LogInformation("Posting infrastructure pull request to {Url}", postUrl);
        var httpClient = httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.PostAsync(postUrl, postBody);

        // get the pull request id
        logger.LogInformation("Getting infrastructure pull request url");
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
        logger.LogInformation("Infrastructure pull request url is {PullRequestUrl}", pullRequestUrl);

        return new PullRequestValueObject(workspaceAcronym, pullRequestUrl, int.Parse(pullRequestId));
    }

    public async Task AutoApproveInfrastructurePullRequest(int pullRequestId, string workspaceAcronym)
    {
        var patchContent = BuildPullRequestPatchBody(workspaceAcronym);
        var patchUrl =
            $"{resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}/{pullRequestId}?api-version={resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        const int retryAmount = 5;
        var retryPolicy = Policy
            .Handle<AutoApproveIncompleteException>()
            .WaitAndRetryAsync(retryAmount, retryAttempt =>
                    TimeSpan.FromSeconds(1),
                (exception, _, _, _) =>
                {
                    logger.LogWarning(exception, "Auto-approve infrastructure pull request failed, retrying");
                });

        await retryPolicy.ExecuteAsync(async ct => { await SendAutoApprovePatchRequestAsync(patchUrl, patchContent); },
            CancellationToken.None);
    }

    public async Task SendAutoApprovePatchRequestAsync(string patchUrl, StringContent patchContent)
    {
        logger.LogInformation("Patching auto-approve infrastructure pull request to {Url}", patchUrl);
        var httpClient = httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.PatchAsync(patchUrl, patchContent);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Could not auto-approve infrastructure pull request {PullRequestUrl}", patchUrl);
            var content = await response.Content.ReadAsStringAsync();
            logger.LogError("Error: {Error}", content);
            throw new AutoApproveException($"Could not auto-approve infrastructure pull request {patchUrl}");
        }

        logger.LogInformation("Infrastructure pull request {PullRequestUrl} auto-approved", patchUrl);

        // Check that the json content of the response has an object "closedBy"
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonContent = JsonSerializer.Deserialize<JsonNode>(responseContent);
        if (jsonContent?["closedBy"] is null)
        {
            logger.LogError("Infrastructure pull request {PullRequestUrl} was not auto-approved", patchUrl);
            throw new AutoApproveIncompleteException($"Infrastructure pull request {patchUrl} was not auto-approved");
        }
    }

    private StringContent BuildPullRequestPatchBody(string workspaceAcronym)
    {
        logger.LogInformation(
            "Building infrastructure pull request patch body for complete by user {ClientId}",
            resourceProvisionerConfiguration.InfrastructureRepository.AzureDevOpsConfiguration.ClientId);
        var patchData = new JsonObject
        {
            ["status"] = "completed",
            ["lastMergeSourceCommit"] = new JsonObject
            {
                ["commitId"] = GetBranchLastCommitId(workspaceAcronym)
            },
            ["completionOptions"] = new JsonObject
            {
                ["deleteSourceBranch"] = false,
                ["mergeCommitMessage"] = $"[{workspaceAcronym}] Auto-merged by ResourceProvisioner"
            }
        };
        var patchBody = new StringContent(JsonSerializer.Serialize(patchData), Encoding.UTF8, "application/json");
        return patchBody;
    }

    public virtual string GetBranchLastCommitId(string branchName)
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(resourceProvisionerConfiguration);
        using var repo = new Repository(repositoryPath);
        var branch = repo.Branches[branchName];

        if (branch is null)
        {
            logger.LogError("Branch {BranchName} does not exist in {RepositoryPath}", branchName, repositoryPath);
            throw new NullReferenceException($"Branch {branchName} does not exist in {repositoryPath}");
        }

        return branch.Tip.Sha;
    }

    private string BuildPullRequestUrl(string pullRequestId)
    {
        return $"{resourceProvisionerConfiguration.InfrastructureRepository.PullRequestBrowserUrl}/{pullRequestId}";
    }

    private StringContent BuildPullRequestPostBody(string workspaceAcronym)
    {
        var postData = new JsonObject
        {
            ["sourceRefName"] = $"refs/heads/{workspaceAcronym}",
            ["targetRefName"] = $"refs/heads/{resourceProvisionerConfiguration.InfrastructureRepository.MainBranch}",
            ["title"] = $"[{workspaceAcronym}] Infrastructure changes",
            ["description"] = $"[{workspaceAcronym}] Infrastructure changes",
        };
        var postBody = new StringContent(JsonSerializer.Serialize(postData), Encoding.UTF8, "application/json");
        return postBody;
    }

    public async Task FetchRepositoriesAndCheckoutProjectBranch(TerraformWorkspace terraformWorkspace)
    {
        await FetchModuleRepository(terraformWorkspace.Version);
        await FetchInfrastructureRepository();
        await CheckoutInfrastructureBranch(terraformWorkspace.Acronym!);
    }

    public async Task<List<RepositoryUpdateEvent>> ExecuteResourceRuns(CreateResourceRunCommand command, string username)
    {
        var repositoryUpdateEvents = new List<RepositoryUpdateEvent>();

        //await ValidateWorkspaceVersion(command.Workspace);


        // Execute each module but make sure the `new-project-template` module is first for creation
        command.Templates = command.Templates.OrderBy(x => x.Name != TerraformTemplate.NewProjectTemplate).ToList();

        foreach (var resourcetemplate in command.Templates)
        {
            var result = await ExecuteResourceRun(resourcetemplate, command, username);
            repositoryUpdateEvents.Add(result);
        }

        return repositoryUpdateEvents;
    }

    
    public async Task<RepositoryUpdateEvent> ExecuteResourceRun(TerraformTemplate resourceTemplate, CreateResourceRunCommand command, string username)
    {
        try
        {            
            if (resourceTemplate.Status == TerraformStatus.DeleteRequested)
            {
                if (resourceTemplate.Name == TerraformTemplate.NewProjectTemplate)
                {
                    await terraformService.DeleteWorkspaceAsync(command.Workspace, command.ResourceGroupName);
                }
                else
                {
                    await terraformService.DeleteTemplateAsync(resourceTemplate.Name, command.Workspace);
                }
            }
            else if (resourceTemplate.Status == TerraformStatus.CreateRequested || command.UpdateWorkspaceVersion)
            {
                await terraformService.CopyTemplateAsync(resourceTemplate.Name, command);
                await ExtractVariables(resourceTemplate, command);
            }
            else
            {
                await ExtractVariables(resourceTemplate, command);
            }

            await CommitTerraformTemplate(resourceTemplate, username);

            return new RepositoryUpdateEvent()
            {
                Message =
                    $"Successfully created resource run for [{command.Workspace.Version}]{resourceTemplate.Name} in {command.Workspace.Acronym} with a template status of {resourceTemplate.Status}",
                StatusCode = MessageStatusCode.Success
            };
        }
        catch (NoChangesDetectedException)
        {
            return new RepositoryUpdateEvent()
            {
                Message =
                    $"No changes detected after resource run for [{command.Workspace.Version}]{resourceTemplate.Name} in {command.Workspace.Acronym}",
                StatusCode = MessageStatusCode.NoChangesDetected
            };
        }
        catch (Exception e)
        {
            logger.LogError(e,
                "Error while creating resource run for [{ModuleVersion}]{ModuleName} in {WorkspaceAcronym}",
                command.Workspace.Version, resourceTemplate.Name, command.Workspace.Acronym);

            return new RepositoryUpdateEvent()
            {
                Message =
                    $"Error creating resource run for [{command.Workspace.Version}]{resourceTemplate.Name} in {command.Workspace.Acronym}",
                StatusCode = MessageStatusCode.Error
            };
        }
    }

    private async Task ExtractVariables(TerraformTemplate template, CreateResourceRunCommand command)
    {
        await terraformService.ExtractVariables(template.Name, command);
        switch (template.Name)
        {
            case TerraformTemplate.NewProjectTemplate:
                await terraformService.ExtractBackendConfig(command.Workspace.Acronym!);
                break;
            case TerraformTemplate.VariableUpdate:
                await terraformService.ExtractAllVariables(command);
                break;
        }
    }

    private async Task<string> GetExistingPullRequestId(string workspaceAcronym)
    {
        logger.LogInformation("Pull request already exists, fetching pull request id");
        var url =
            $"{resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}?searchCriteria.status=active&searchCriteria.sourceRefName=refs/heads/{workspaceAcronym}&api-version={resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

        using var httpClient = httpClientFactory.CreateClient("InfrastructureHttpClient");
        var response = await httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonNode>(content);

        return data?["value"]?
                   .AsArray()
                   .FirstOrDefault(node => node?["sourceRefName"]?.ToString() == $"refs/heads/{workspaceAcronym}")?
                   .AsObject()["pullRequestId"]?.ToString() ??
               throw new NullReferenceException(
                   $"Could not get existing pull request id for workspace {workspaceAcronym}");
    }

    private void CleanUpEnvironment()
    {
        try
        {
            logger.LogInformation("Deleting temporary directory {Directory} for resource run",
                DirectoryUtils.tempDirectory);
            var tempPath = DirectoryUtils.GetTempDirectoryPath(resourceProvisionerConfiguration);
            var dir = new DirectoryInfo(tempPath);
            DirectoryUtils.NormalizeAndDelete(dir);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while cleaning up environment");
        }
    }
}