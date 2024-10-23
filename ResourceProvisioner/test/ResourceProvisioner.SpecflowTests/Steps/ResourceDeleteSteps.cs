using Datahub.Shared;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Domain.Enums;
using ResourceProvisioner.Domain.Events;
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
    public void ThenTheDeleteTemplateAsyncMethodShouldBeInvoked()
    {
        var templateName = scenarioContext.Get<string>("templateName");
        var terraformService = scenarioContext.Get<ITerraformService>("terraformService");
        terraformService.Received().DeleteTemplateAsync(templateName, Arg.Any<TerraformWorkspace>());
    }

    [Then(@"the CopyTemplateAsync method should not be invoked")]
    public void ThenTheCopyTemplateAsyncMethodShouldNotBeInvoked()
    {
        var terraformService = scenarioContext.Get<ITerraformService>("terraformService");
        terraformService.DidNotReceive().CopyTemplateAsync(Arg.Any<string>(), Arg.Any<TerraformWorkspace>());
    }

    [Then(@"the result should be a successful RepositoryUpdateEvent")]
    public void ThenTheResultShouldBeASuccessfulRepositoryUpdateEvent()
    {
        var result = scenarioContext.Get<RepositoryUpdateEvent>("result");
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(MessageStatusCode.Success);
    }
}