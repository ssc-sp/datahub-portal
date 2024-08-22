using System.Net;
using System.Text;
using System.Text.Json;
using Reqnroll.BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Functions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.SpecflowTests.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("azure-devops-pull-request")]
    public void BeforeScenarioRequiringAzureDevopsPullRequest(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddUserSecrets<Hooks>()
            .Build();

        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);

        var httpMessageHandler = new TestHttpMessageHandler();
        httpMessageHandler.AddResponse(AdoPullRequestResponseMessage());
        httpMessageHandler.AddResponse(AdoPullRequestResponseMessage());
        httpMessageHandler.AddResponse(AdoPullRequestResponseMessage());
        httpMessageHandler.AddResponse(AdoPullRequestResponseMessage());
        httpMessageHandler.AddResponse(AdoPullRequestResponseMessage(true));

        var httpClient = new HttpClient(httpMessageHandler);

        var httpClientFactorySubstitute = Substitute.For<IHttpClientFactory>();
        httpClientFactorySubstitute
            .CreateClient(Arg.Any<string>())
            .Returns(httpClient);

        var repositoryService = Substitute.ForPartsOf<RepositoryService>(
            httpClientFactorySubstitute,
            Substitute.For<ILogger<RepositoryService>>(),
            resourceProvisionerConfiguration,
            Substitute.For<ITerraformService>());

        repositoryService
            .GetBranchLastCommitId(Arg.Any<string>())
            .ReturnsForAnyArgs("1234567890");

        // register dependencies
        objectContainer.RegisterInstanceAs(resourceProvisionerConfiguration);
        objectContainer.RegisterInstanceAs(repositoryService);
    }

    private HttpResponseMessage AdoPullRequestResponseMessage(bool includeClosedBy = false)
    {
        var body = new
        {
            respository = new
            {
                id = "ae4d3b3d-4b3d-4b3d-4b3d-4b3d4b3d4b3d",
                name = "test-repo"
            },
            pullRequestId = 1,
            status = "active",
            closedBy = includeClosedBy
                ? new
                {
                    id = "ae4d3b3d-4b3d-4b3d-4b3d-4b3d4b3d4b3d",
                    displayName = "test-user"
                }
                : null
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        return responseMessage;
    }

    [BeforeScenario("infra-repository")]
    public void BeforeScenarioRequiringInfraRepository(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddUserSecrets<Hooks>()
            .Build();

        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);

        var terraformServiceLoggerSubstitute = Substitute.For<ILogger<TerraformService>>();
        var terraformService = new TerraformService(
            terraformServiceLoggerSubstitute,
            resourceProvisionerConfiguration,
            configuration);

        var httpClientFactorySubstitute = Substitute.For<IHttpClientFactory>();
        httpClientFactorySubstitute
            .CreateClient(Arg.Any<string>())
            .Returns(new HttpClient());
        var repositoryServiceLoggerSubstitute = Substitute.For<ILogger<RepositoryService>>();
        var repositoryService = new RepositoryService(
            httpClientFactorySubstitute,
            repositoryServiceLoggerSubstitute,
            resourceProvisionerConfiguration,
            terraformService);

        // register dependencies
        objectContainer.RegisterInstanceAs(resourceProvisionerConfiguration);
        objectContainer.RegisterInstanceAs<ITerraformService>(terraformService);
        objectContainer.RegisterInstanceAs<IRepositoryService>(repositoryService);
    }

    [AfterScenario("infra-repository")]
    public void AfterScenarioRequiringInfraRepository(IObjectContainer objectContainer)
    {
        var resourceProvisionerConfiguration = objectContainer.Resolve<ResourceProvisionerConfiguration>();
        var expectedClonePath = Path.Join(Environment.CurrentDirectory,
            resourceProvisionerConfiguration.InfrastructureRepository.LocalPath);
        DirectoryUtils.VerifyDirectoryDoesNotExist(expectedClonePath);

        if (objectContainer.Resolve<IRepositoryService>() is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [BeforeScenario("resource-run-function")]
    public void BeforeScenarioRequiringResourceRunFunction(IObjectContainer objectContainer,
        ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);

        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger<ResourceRunRequest>().Returns(Substitute.For<ILogger<ResourceRunRequest>>());
        var substituteRepositoryService = Substitute.For<IRepositoryService>();
        var resourceRunRequest = new ResourceRunRequest(
            loggerFactory,
            substituteRepositoryService);

        // register dependencies
        objectContainer.RegisterInstanceAs(resourceProvisionerConfiguration);
        objectContainer.RegisterInstanceAs(resourceRunRequest);
    }
}