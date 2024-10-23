using Datahub.Shared;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Domain.Enums;
using ResourceProvisioner.Domain.Events;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.SpecflowTests.Steps;

[Binding]
public class ResourceDeleteSteps(ScenarioContext scenarioContext)
{
    [Given(@"a repository service with a stubbed CommitTerraformTemplate method")]
    public void GivenARepositoryServiceWithAStubbedCommitTerraformTemplateMethod()
    {
        var terraformService = Substitute.For<ITerraformService>();
        var repositoryService = Substitute.ForPartsOf<RepositoryService>(
            Substitute.For<IHttpClientFactory>(),
            Substitute.For<ILogger<RepositoryService>>(),
            Substitute.For<ResourceProvisionerConfiguration>(),
            terraformService
        );

        repositoryService
            .Configure()
            .CommitTerraformTemplate(Arg.Any<TerraformTemplate>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        scenarioContext.Add("repositoryService", repositoryService);
        scenarioContext.Add("terraformService", terraformService);
    }

    [Given(@"a template is set to be deleted")]
    public void GivenATemplateIsSetToBeDeleted()
    {
        const string templateName = "template";
        var terraformTemplate = new TerraformTemplate(templateName, TerraformStatus.DeleteRequested);
        scenarioContext.Add("terraformTemplate", terraformTemplate);
        scenarioContext.Add("templateName", templateName);
    }

    [When(@"the ExecuteResourceRun method is invoked")]
    public async Task WhenTheExecuteResourceRunMethodIsInvoked()
    {
        var repositoryService = scenarioContext.Get<RepositoryService>("repositoryService");
        var terraformTemplate = scenarioContext.Get<TerraformTemplate>("terraformTemplate");
        var result = await repositoryService.ExecuteResourceRun(terraformTemplate, Substitute.For<TerraformWorkspace>(),
            "test@username");
        scenarioContext.Add("result", result);
    }

    [Then(@"the DeleteTemplateAsync method should be invoked")]
    public async Task ThenTheDeleteTemplateAsyncMethodShouldBeInvoked()
    {
        var templateName = scenarioContext.Get<string>("templateName");
        var terraformService = scenarioContext.Get<ITerraformService>("terraformService");
        await terraformService.Received().DeleteTemplateAsync(templateName, Arg.Any<TerraformWorkspace>());
    }

    [Then(@"the CopyTemplateAsync method should not be invoked")]
    public async Task ThenTheCopyTemplateAsyncMethodShouldNotBeInvoked()
    {
        var terraformService = scenarioContext.Get<ITerraformService>("terraformService");
        await terraformService.DidNotReceive().CopyTemplateAsync(Arg.Any<string>(), Arg.Any<TerraformWorkspace>());
    }

    [Then(@"the result should be a successful RepositoryUpdateEvent")]
    public void ThenTheResultShouldBeASuccessfulRepositoryUpdateEvent()
    {
        var result = scenarioContext.Get<RepositoryUpdateEvent>("result");
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(MessageStatusCode.Success);
    }

    [Given(@"a terraform service with a stubbed WriteDeletedFile method")]
    public void GivenATerraformServiceWithAStubbedWriteDeletedFileMethod()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Hooks.Hooks>()
            .AddJsonFile("appsettings.test.json", optional: true)
            .Build();

        var resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        configuration.Bind(resourceProvisionerConfiguration);

        var terraformService = Substitute.ForPartsOf<TerraformService>(
            Substitute.For<ILogger<TerraformService>>(),
            resourceProvisionerConfiguration,
            configuration
        );

        terraformService
            .Configure()
            .WriteDeletedFile(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        scenarioContext.Add("terraformService", terraformService);
        scenarioContext.Add("resourceProvisionerConfiguration", resourceProvisionerConfiguration);
    }

    [Given(@"a template name of ""(.*)""")]
    public void GivenATemplateNameOf(string templateName)
    {
        scenarioContext.Add("templateName", templateName);
    }

    [Given(@"a few files starting with the template name exist in the project path")]
    public void GivenAFewFilesStartingWithTheTemplateNameExistInTheProjectPath()
    {
        var resourceProvisionerConfiguration =
            scenarioContext.Get<ResourceProvisionerConfiguration>("resourceProvisionerConfiguration");
        var terraformWorkspace = scenarioContext.Get<TerraformWorkspace>("terraformWorkspace");
        var templateName = scenarioContext.Get<string>("templateName");
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, terraformWorkspace.Acronym);

        Directory.CreateDirectory(projectPath);
        File.WriteAllText(Path.Join(projectPath, $"{templateName}.auto.tfvars.json"), "test");
        File.WriteAllText(Path.Join(projectPath, $"{templateName}.tf"), "test");
        File.WriteAllText(Path.Join(projectPath, $"{templateName}.random"), "test");
    }

    [When(@"the DeleteTemplateAsync method is invoked")]
    public async Task WhenTheDeleteTemplateAsyncMethodIsInvoked()
    {
        var terraformService = scenarioContext.Get<TerraformService>("terraformService");
        var templateName = scenarioContext.Get<string>("templateName");
        var terraformWorkspace = scenarioContext.Get<TerraformWorkspace>("terraformWorkspace");

        await terraformService.DeleteTemplateAsync(templateName, terraformWorkspace);
    }

    [Then(@"the files starting with the template name should be deleted")]
    public void ThenTheFilesStartingWithTheTemplateNameShouldBeDeleted()
    {
        var resourceProvisionerConfiguration =
            scenarioContext.Get<ResourceProvisionerConfiguration>("resourceProvisionerConfiguration");
        var terraformWorkspace = scenarioContext.Get<TerraformWorkspace>("terraformWorkspace");
        var templateName = scenarioContext.Get<string>("templateName");
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, terraformWorkspace.Acronym);

        File.Exists(Path.Join(projectPath, $"{templateName}.auto.tfvars.json")).Should().BeFalse();
        File.Exists(Path.Join(projectPath, $"{templateName}.tf")).Should().BeFalse();
        File.Exists(Path.Join(projectPath, $"{templateName}.random")).Should().BeFalse();
    }

    [Then(@"the WriteDeletedFile method should be invoked")]
    public async Task ThenTheWriteDeletedFileMethodShouldBeInvoked()
    {
        var terraformService = scenarioContext.Get<TerraformService>("terraformService");
        await terraformService.Received().WriteDeletedFile(Arg.Any<string>(), Arg.Any<string>());
    }

    [Given(@"a terraform workspace with acronym ""(.*)""")]
    public void GivenATerraformWorkspaceWithAcronym(string acronym)
    {
        var terraformWorkspace = new TerraformWorkspace()
        {
            Acronym = acronym
        };
        scenarioContext.Add("terraformWorkspace", terraformWorkspace);
    }
}