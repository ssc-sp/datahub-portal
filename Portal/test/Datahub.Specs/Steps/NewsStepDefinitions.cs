using Datahub.Specs.PageObjects;
using FluentAssertions;
using Microsoft.Playwright;
using Playwright.Axe;
using Reqnroll;

namespace Datahub.Specs.Steps;

[Binding]
public sealed class NewsStepDefinitions
{
    private readonly NewsPageObject _newsPageObject;

    public NewsStepDefinitions(NewsPageObject newsPageObject)
    {
        _newsPageObject = newsPageObject;
    }

    [Given(@"the user is on the News page")]
    public async Task GivenTheUserIsOnTheNewsPage()
    {
        await _newsPageObject.NavigateAsync();
        await _newsPageObject.ValidateLocationAsync();
    }

    [Then(@"the workspace page should be without accessibility errors")]
    public async Task ThenTheWorkspacePageShouldBeWithoutAccessibilityErrors()
    {
        await _newsPageObject.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var axeResults = await _newsPageObject.Page.RunAxe();
        axeResults.Violations.Should().BeEmpty();
    }
}

[Binding]
public sealed class AdminNewsStepDefinitions
{
    private readonly NewsPageObjectAdmin _newsPageObjectAdmin;

    public AdminNewsStepDefinitions(NewsPageObjectAdmin newsPageObjectAdmin)
    {
        _newsPageObjectAdmin = newsPageObjectAdmin;
    }

    [Given(@"the user is on the News page as Admin")]
    public async Task GivenTheUserIsOnTheNewsPageAsAdmin()
    {
        await _newsPageObjectAdmin.NavigateAsync();
        await _newsPageObjectAdmin.ValidateLocationAsync();
    }

    [Then(@"the create news button is available")]
    public async Task ThenTheCreateNewsButtonIsAvailable()
    {
        await _newsPageObjectAdmin.AssertCreateNewsButtonExistsAsync();
    }


    [Then(@"the admin clicks the create button")]
    public void ThenTheAdminClicksTheCreateButton()
    {
        throw new PendingStepException();
    }

    [Given(@"the admin clicks the create button")]
    public async Task GivenTheAdminClicksTheCreateButton()
    {
        await _newsPageObjectAdmin.ClickCreateNewsButtonAsync();
    }

    [Then(@"the edit news page is loaded")]
    public void ThenTheEditNewsPageIsLoaded()
    {
        _newsPageObjectAdmin.Page.Url.Should().EndWith($"/news/edit/new");
    }
}
