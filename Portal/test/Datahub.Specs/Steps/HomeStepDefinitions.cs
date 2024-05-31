using Datahub.Specs.PageObjects;
using FluentAssertions;
using Microsoft.Playwright;
using Playwright.Axe;
using Reqnroll;

namespace Datahub.Specs.Steps;

[Binding]
// ReSharper disable once InconsistentNaming
public sealed class HomeStepDefinitions
{

    private readonly HomePageObject _homePageObject;

    public HomeStepDefinitions(HomePageObject homePageObject)
    {
        _homePageObject = homePageObject;
    }

    [Then(@"the user is on the home page")]
    public async Task ThenTheUserIsOnTheHomePage()
    {
        await _homePageObject.NavigateAsync();
        await _homePageObject.ValidateLocationAsync();
    }

    [Then(@"there should be no accessibility errors")]
    public async Task ThenThereShouldBeNoAccessibilityErrors()
    {
        await _homePageObject.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        var axeResults = await _homePageObject.Page.RunAxe();
        axeResults.Violations.Should().BeEmpty();
    }
}