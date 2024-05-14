using Datahub.Specs.PageObjects;
using FluentAssertions;
using Microsoft.Playwright;
using Playwright.Axe;
using Reqnroll;

namespace Datahub.Specs.Steps;

[Binding]
public sealed class WorkspaceStepDefinitions
{
    private readonly WorkspacePageObject _wsPageObject;

    public WorkspaceStepDefinitions(WorkspacePageObject homePageObject)
    {
        _wsPageObject = homePageObject;
    }

    [Given(@"the user is on the workspace page")]
    public async Task GivenTheUserIsOnTheWorkspacePage()
    {
        await _wsPageObject.NavigateAsync();
        await _wsPageObject.ValidateLocationAsync();
    }

    [Then(@"the workspace page should be without accessibility errors")]
    public async Task ThenTheWorkspacePageShouldBeWithoutAccessibilityErrors()
    {
        await _wsPageObject.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var axeResults = await _wsPageObject.Page.RunAxe();
        axeResults.Violations.Should().BeEmpty();
    }
}
