using Datahub.Specs.PageObjects;
using FluentAssertions;
using Microsoft.Playwright;
using Playwright.Axe;

namespace Datahub.Specs.Steps;

[Binding]
public class ResourcesStepDefinitions
{
    private readonly ResourcesPageObject _resPageObject;

    public ResourcesStepDefinitions(ResourcesPageObject resPageObject)
    {
        _resPageObject = resPageObject;
    }

    [Given(@"the user is on the resource page")]
    public async Task GivenTheUserIsOnTheResourcePage()
    {
        await _resPageObject.NavigateAsync();
        _resPageObject.Page.Url.Should().EndWith("/resources");
    }

    [Then(@"the resource page should be without accessibility errors")]
    public async Task ThenTheResourcePageShouldBeWithoutAccessibilityErrors()
    {
        var axeResults = await _resPageObject.RunAxe();
        axeResults.Violations.Should().BeEmpty();
    }
}
