using Datahub.Specs.PageObjects;
using FluentAssertions;
using Reqnroll;

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
        await _resPageObject.ValidateLocationAsync();
    }

    [Then(@"the resource page should be without accessibility errors")]
    public async Task ThenTheResourcePageShouldBeWithoutAccessibilityErrors()
    {
        var axeResults = await _resPageObject.RunAxe();
        axeResults.Violations.Should().BeEmpty();
    }
}
