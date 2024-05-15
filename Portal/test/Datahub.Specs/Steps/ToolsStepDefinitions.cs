using Datahub.Specs.PageObjects;
using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;

namespace Datahub.Specs.Steps;

[Binding]
public class ToolsStepDefinitions
{
    private readonly AdminToolsPageObject _toolsPageObject;

    public ToolsStepDefinitions(AdminToolsPageObject toolsPageObject)
    {
        _toolsPageObject = toolsPageObject;
    }

    [Given(@"an admin is in the tools page")]
    public async Task GivenTheUserIsInTheToolsPage()
    {
        await _toolsPageObject.NavigateAsync();
        await _toolsPageObject.ValidateLocationAsync();
    }

    [Given(@"the sidebar contains the tools button")]
    public async Task GivenTheSidebarContainsTheToolsButton()
    {
        await _toolsPageObject.AssertToolsButtonExistsAsync();
    }

    static List<string> ToolLabels = new() { "Diagnostics", "Statistics", "Health", "Users", "Email" };

    [Then(@"a set of tools are available")]
    public async Task ThenASetOfToolsAreAvailable()
    {
        var page = _toolsPageObject.Page;
        foreach(var label in ToolLabels)
        {
            (await page.QuerySelectorAsync($"h5:has-text(\"{label}\")")).Should().NotBeNull();
        }
    }

    [Given(@"the admin access is switch off")]
    public async Task GivenTheAdminAccessIsSwitchOff()
    {
        var page = _toolsPageObject.Page;
        await page.ClickAsync("div.d-flex.flex-column.align-start.gap-0");
        await page.GetByLabel("Admin Access Enabled").UncheckAsync(new LocatorUncheckOptions() { NoWaitAfter = true });
    }

    [Then(@"a set of tools are not available")]
    public async Task ThenASetOfToolsAreNotAvailable()
    {
        var page = _toolsPageObject.Page;
        foreach (var label in ToolLabels)
        {
            (await page.QuerySelectorAsync($"h5:has-text(\"{label}\")")).Should().BeNull();
        }
    }
}
