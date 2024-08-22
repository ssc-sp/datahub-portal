using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Services;
using Xunit;

namespace ResourceProvisioner.SpecflowTests.Steps;

[Binding]
public sealed class AzureDevopsAutocompletePullRequestSteps(
    ScenarioContext scenarioContext,
    ResourceProvisionerConfiguration resourceProvisionerConfiguration,
    RepositoryService repositoryService)
{
    [Given(@"a pull request id of (.*)")]
    public void GivenAPullRequestIdOf(int p0)
    {
        scenarioContext["pullRequestId"] = p0;
    }

    [Given(@"a workspace acronym ""(.*)""")]
    public void GivenAWorkspaceAcronym(string p0)
    {
        scenarioContext["workspaceAcronym"] = p0;
    }

    [When(@"a pull request patch request is sent")]
    public async Task WhenAPullRequestPatchRequestIsSent()
    {
        var pullRequestId = scenarioContext["pullRequestId"] as int? ?? default;
        try
        {
            var patchUrl = $"{resourceProvisionerConfiguration.InfrastructureRepository.PullRequestUrl}/{pullRequestId}?api-version={resourceProvisionerConfiguration.InfrastructureRepository.ApiVersion}";

            await repositoryService.SendAutoApprovePatchRequestAsync(patchUrl, new StringContent("test"));
        }
        catch (Exception e)
        {
            scenarioContext["exception"] = e;
        }
    }

    [Then(@"a successful response should be returned with no exceptions")]
    public void ThenASuccessfulResponseShouldBeReturnedWithNoExceptions()
    {
        try
        {
            var exception = scenarioContext["exception"] as Exception;
            Assert.Null(exception);   
        } catch(KeyNotFoundException e)
        {
            // Simply pass as no exception is expected
        }
    }

    [When(@"the pull request is not closed")]
    public async Task WhenThePullRequestIsNotClosed()
    {
        var pullRequestId = scenarioContext["pullRequestId"] as int? ?? default;
        var workspaceAcronym = scenarioContext["workspaceAcronym"] as string;

        try
        {
            await repositoryService.AutoApproveInfrastructurePullRequest(pullRequestId, workspaceAcronym!);
        }
        catch (Exception e)
        {
            scenarioContext["exception"] = e;
        }
    }

    [Then(@"an AutoApproveIncompleteException should be thrown")]
    public void ThenAnAutoApproveIncompleteExceptionShouldBeThrown()
    {
        var exception = scenarioContext["exception"] as Exception;
        Assert.NotNull(exception);
        Assert.IsType<AutoApproveIncompleteException>(exception);
    }

    [When(@"an auto approve pull request is expected")]
    public async Task WhenAnAutoApprovePullRequestIsExpected()
    {
        var pullRequestId = scenarioContext["pullRequestId"] as int? ?? default;
        var workspaceAcronym = scenarioContext["workspaceAcronym"] as string;
        
        try
        {
            await repositoryService.AutoApproveInfrastructurePullRequest(pullRequestId, workspaceAcronym!);
        }
        catch (Exception e)
        {
            scenarioContext["exception"] = e;
        }
    }
}