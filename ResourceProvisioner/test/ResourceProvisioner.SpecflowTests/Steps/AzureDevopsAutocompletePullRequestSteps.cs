using Reqnroll;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Infrastructure.Services;
using Xunit;

namespace ResourceProvisioner.SpecflowTests.Steps;

[Binding]
public sealed class AzureDevopsAutocompletePullRequestSteps(
    ScenarioContext scenarioContext,
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
        var workspaceAcronym = scenarioContext["workspaceAcronym"] as string;

        var result = await repositoryService.AutoApproveInfrastructurePullRequest(pullRequestId, workspaceAcronym!);
        scenarioContext["result"] = result;
    }

    [Then(@"a successful response should be returned")]
    public void ThenASuccessfulResponseShouldBeReturned()
    {
        var result = scenarioContext["result"] as bool? ?? false;
        Assert.True(result);
    }
}