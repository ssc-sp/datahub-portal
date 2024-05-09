using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Functions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.SpecflowTests.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("infra-repository")]
    public void BeforeScenarioRequiringInfraRepository(IObjectContainer objectContainer, ScenarioContext scenarioContext)
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
        var expectedClonePath = Path.Join(Environment.CurrentDirectory, resourceProvisionerConfiguration.InfrastructureRepository.LocalPath);
        DirectoryUtils.VerifyDirectoryDoesNotExist(expectedClonePath);
        
        if (objectContainer.Resolve<IRepositoryService>() is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
    }
    
    [BeforeScenario("resource-run-function")]
    public void BeforeScenarioRequiringResourceRunFunction(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);
        
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger<ResourceRunRequest>().Returns(Substitute.For<ILogger<ResourceRunRequest>>());
        var createResourceRunCommandHandler = Substitute.For<CreateResourceRunCommandHandler>(
            Substitute.For<ILogger<CreateResourceRunCommandHandler>>(),
            Substitute.For<IRepositoryService>()
            );
        var resourceRunRequest = new ResourceRunRequest(
            loggerFactory,
            createResourceRunCommandHandler);
        
        // register dependencies
        objectContainer.RegisterInstanceAs(resourceProvisionerConfiguration);
        objectContainer.RegisterInstanceAs(resourceRunRequest);
    }
    
}