using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Domain.Enums;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Domain.ValueObjects;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Version = System.Version;

namespace ResourceProvisioner.Infrastructure.UnitTests.Services;

using static Testing;

public class RepositoryServiceTests
{
    [SetUp]
    public void RunBeforeEachTest()
    {
        var localModuleClonePath =
            Path.Join(Environment.CurrentDirectory, _configuration["ModuleRepository:LocalPath"]);
        var localInfrastructureClonePath = Path.Join(Environment.CurrentDirectory,
            _configuration["InfrastructureRepository:LocalPath"]);

        VerifyDirectoryDoesNotExist(localModuleClonePath);
        VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
    }

    [Test]
    public async Task ShouldFetchModuleRepository()
    {
        var expectedClonePath = Path.Join(Environment.CurrentDirectory, _configuration["ModuleRepository:LocalPath"]);

        Assert.That(Directory.Exists(expectedClonePath), Is.False);

        await _repositoryService.FetchModuleRepository();

        Assert.That(Directory.Exists(expectedClonePath), Is.True);
    }

    [Test]
    public async Task ShouldFetchModuleRepositoryAndOverwriteExisting()
    {
        var expectedClonePath = Path.Join(Environment.CurrentDirectory, _configuration["ModuleRepository:LocalPath"]);

        Assert.That(Directory.Exists(expectedClonePath), Is.False);

        await _repositoryService.FetchModuleRepository();

        Assert.That(Directory.Exists(expectedClonePath), Is.True);

        // Write a new file
        var repository = new Repository(expectedClonePath);
        var fileName = $"{Guid.NewGuid()}.txt";
        const string content = "Commit this!";
        await File.WriteAllTextAsync(Path.Combine(repository.Info.WorkingDirectory, fileName), content);
        Assert.That(File.Exists(Path.Combine(repository.Info.WorkingDirectory, fileName)), Is.True);

        // Overwrite the existing repository with a new one
        await _repositoryService.FetchModuleRepository();
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(Path.Combine(repository.Info.WorkingDirectory, fileName)), Is.False);
            Assert.That(Directory.Exists(expectedClonePath), Is.True);
        });
    }

    [Test]
    public async Task ShouldFetchInfrastructureRepository()
    {
        var repositoryLocalPath = _configuration["InfrastructureRepository:LocalPath"];
        var expectedClonePath = Path.Join(Environment.CurrentDirectory, repositoryLocalPath);

        Assert.That(Directory.Exists(expectedClonePath), Is.False);

        await _repositoryService.FetchInfrastructureRepository();

        Assert.That(Directory.Exists(expectedClonePath), Is.True);
    }

    [Test]
    public async Task ShouldCheckoutProjectBranch()
    {
        var repositoryLocalPath = _configuration["InfrastructureRepository:LocalPath"];
        var expectedClonePath = Path.Join(Environment.CurrentDirectory, repositoryLocalPath);

        Assert.That(Directory.Exists(expectedClonePath), Is.False);

        await _repositoryService.FetchInfrastructureRepository();

        Assert.That(Directory.Exists(expectedClonePath), Is.True);

        await _repositoryService.CheckoutInfrastructureBranch(ProjectAcronym);

        var repository = new Repository(expectedClonePath);
        Assert.That(repository.Head.FriendlyName, Is.EqualTo(ProjectAcronym));
    }

    [Test]
    public async Task ShouldFetchBothRepositoriesAndCheckoutProjectBranch()
    {
        var moduleRepositoryPath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        var infrastructureRepositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);

        Assert.That(Directory.Exists(moduleRepositoryPath), Is.False);
        Assert.That(Directory.Exists(infrastructureRepositoryPath), Is.False);

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(ProjectAcronym);

        Assert.That(Directory.Exists(moduleRepositoryPath), Is.True);
        Assert.That(Directory.Exists(infrastructureRepositoryPath), Is.True);

        var repository = new Repository(infrastructureRepositoryPath);
        Assert.That(repository.Head.FriendlyName, Is.EqualTo(ProjectAcronym));
    }

    [Test]
    public async Task ShouldCommitTerraformTemplateChanges()
    {
        var repository = InitializeTestInfrastructureRepository();
        CreateFakeFileInTestProject();

        await _repositoryService.CommitTerraformTemplate(TestTemplate, RequestingUser);

        Assert.Multiple(() =>
        {
            Assert.That(repository.Commits.Count(), Is.EqualTo(2));
            Assert.That(repository.Index, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public async Task ShouldExecuteResourceRun()
    {
        InitializeTestInfrastructureRepository();
        var mockTerraformService = SetupMockTerraformService();
        
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());

        var repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
            _resourceProvisionerConfiguration, mockTerraformService);

        var result =
            await repositoryService.ExecuteResourceRun(TestTemplate, TestingWorkspace, RequestingUser);


        Assert.That(result, Is.TypeOf<RepositoryUpdateEvent>());
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(MessageStatusCode.Success));
            Assert.That(result.Message, Contains.Substring(TestTemplate.Name));
            Assert.That(result.Message, Contains.Substring(TestingWorkspace.Version));
            Assert.That(result.Message, Contains.Substring(ProjectAcronym));
        });
    }


    [Test]
    public async Task ShouldReturnNoChangesIfNoCommitsWhenExecutingResourceRun()
    {
        InitializeTestInfrastructureRepository();
        var mockTerraformService = SetupMockTerraformService(true);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());

        var repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
            _resourceProvisionerConfiguration, mockTerraformService);

        var result =
            await repositoryService.ExecuteResourceRun(TestTemplate, TestingWorkspace, RequestingUser);

        Assert.That(result, Is.TypeOf<RepositoryUpdateEvent>());
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(MessageStatusCode.NoChangesDetected));
            Assert.That(result.Message, Contains.Substring(TestTemplate.Name));
            Assert.That(result.Message, Contains.Substring(TestingWorkspace.Version));
            Assert.That(result.Message, Contains.Substring(ProjectAcronym));
        });
    }

    [Test]
    public async Task ShouldExecuteMultipleResourceRun()
    {
        InitializeTestInfrastructureRepository();
        var mockTerraformService = SetupMockTerraformService();

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());
        
        var repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
            _resourceProvisionerConfiguration, mockTerraformService);
        
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var command = GenerateTestCreateResourceRunCommand(
            workspaceAcronym, new List<string>()
            {
                TerraformTemplate.NewProjectTemplate,
                TerraformTemplate.NewProjectTemplate,
                TerraformTemplate.NewProjectTemplate
            });

        var result =
            await repositoryService.ExecuteResourceRuns(command.Templates, command.Workspace, RequestingUser);


        Assert.That(result, Is.TypeOf<List<RepositoryUpdateEvent>>());
        Assert.That(result, Has.Count.EqualTo(command.Templates.Count));

        Assert.Multiple(() =>
        {
            foreach (var repositoryUpdateEvent in result)
            {
                Assert.That(repositoryUpdateEvent.StatusCode, Is.EqualTo(MessageStatusCode.Success),
                    repositoryUpdateEvent.Message);
                Assert.That(repositoryUpdateEvent.Message, Contains.Substring(TerraformTemplate.NewProjectTemplate));
                Assert.That(repositoryUpdateEvent.Message, Contains.Substring(command.Workspace.Version));
                Assert.That(repositoryUpdateEvent.Message, Contains.Substring(workspaceAcronym));
            }
        });
    }

    [Test]
    public async Task ShouldPushInfrastructureChangesToRepository()
    {
        var repository = InitializeTestInfrastructureRepository();
        var mockTerraformService = SetupMockTerraformService();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());
        var repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
            _resourceProvisionerConfiguration, mockTerraformService);

        await repositoryService.FetchRepositoriesAndCheckoutProjectBranch(ProjectAcronym);

        await Task.Run(DeleteAllFilesInTestProject);

        CreateFakeFileInTestProject();
        Commands.Stage(repository, "*");
        repository.Commit("Push test commit", new Signature(RequestingUser, RequestingUser, DateTimeOffset.Now),
            new Signature(RequestingUser, RequestingUser, DateTimeOffset.Now));

        await repositoryService.PushInfrastructureRepository(ProjectAcronym);
    }


    [Test]
    public async Task ShouldCreatePullRequest()
    {
        var fakePullRequestId = new Random().Next(9999999);
        var expectedPullRequestResponse = ExpectedPullRequestResponse(fakePullRequestId);

        var mockTerraformService = SetupMockTerraformService();

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = expectedPullRequestResponse
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        
        var repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
            _resourceProvisionerConfiguration, mockTerraformService);

        var result = await repositoryService.CreateInfrastructurePullRequest(ProjectAcronym, RequestingUser);

        Assert.That(result, Is.TypeOf<PullRequestValueObject>());
        Assert.That(result.Url, Is.EqualTo($"{_configuration["InfrastructureRepository:PullRequestBrowserUrl"]}/{fakePullRequestId}"));
        Assert.That(result.Url.Split('/').Last(), Is.EqualTo(fakePullRequestId.ToString()));
        Assert.That(result.WorkspaceAcronym, Is.EqualTo(ProjectAcronym));
    }

    [Test]
    public async Task ShouldBeAbleToGetModuleVersions()
    {
        var result = await _repositoryService.GetModuleVersions();
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Is.All.InstanceOf<Version>());
    }

    private static StringContent ExpectedPullRequestResponse(int fakePullRequestId)
    {
        var data = new JsonObject
        {
            ["url"] = $"https://dev.azure.com/info/pullRequests/{fakePullRequestId}",
            ["pullRequestId"] = fakePullRequestId,
        };
        var stringContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        return stringContent;
    }

    private static Repository InitializeTestInfrastructureRepository()
    {
        var repositoryPath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, ProjectAcronym);

        VerifyDirectoryDoesNotExist(repositoryPath);
        Directory.CreateDirectory(repositoryPath);
        Directory.CreateDirectory(projectPath);
        Repository.Init(repositoryPath);

        var repository = new Repository(repositoryPath);
        Commands.Stage(repository, "*");
        repository.Commit("Initial commit", new Signature(RequestingUser, RequestingUser, DateTimeOffset.Now),
            new Signature(RequestingUser, RequestingUser, DateTimeOffset.Now));
        return repository;
    }

    private static ITerraformService SetupMockTerraformService(bool doNothing = false)
    {
        var mockTerraformService = new Mock<ITerraformService>();

        mockTerraformService.Setup(tf => tf.CopyTemplateAsync(
                It.IsAny<string>(),
                It.IsAny<TerraformWorkspace>()))
            .Returns(() =>
            {
                if (doNothing)
                {
                    return Task.CompletedTask;
                }

                CreateFakeFileInTestProject();
                return Task.CompletedTask;
            });

        mockTerraformService.Setup(tf => tf.ExtractVariables(It.IsAny<string>(), It.IsAny<TerraformWorkspace>()))
            .Returns(Task.CompletedTask);
        return mockTerraformService.Object;
    }

    private static void CreateFakeFileInTestProject()
    {
        var fileName = $"{Guid.NewGuid()}.tf";
        const string content = "# Commit this!";
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, ProjectAcronym);

        if (!Directory.Exists(projectPath))
        {
            Directory.CreateDirectory(projectPath);
        }

        File.WriteAllText(Path.Join(projectPath, fileName), content);
    }


    private static void DeleteAllFilesInTestProject()
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, ProjectAcronym);
        if (!Directory.Exists(projectPath))
        {
            return;
        }

        var projectDirectory = new DirectoryInfo(projectPath);
        SetAttributesNormal(projectDirectory);

        foreach (var file in projectDirectory.GetFiles())
        {
            file.Delete();
        }
    }
}