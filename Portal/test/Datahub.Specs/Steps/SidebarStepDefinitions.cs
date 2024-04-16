using Datahub.Specs.PageObjects;
using FluentAssertions;
using NUnit.Framework;
using Reqnroll;

namespace Datahub.Specs.Steps;

[Binding]
public sealed class SidebarStepDefinitions
{
    private readonly SideBarPageObject _sideBarPageObject;

    public SidebarStepDefinitions(SideBarPageObject sideBarPageObject)
    {
        _sideBarPageObject = sideBarPageObject;
    }

    [Given(@"the user is on any page")]
    public async Task GivenTheUserIsOnAnyPage()
    {
        await _sideBarPageObject.NavigateAsync();
        await _sideBarPageObject.ValidateLocationAsync();
    }

    [Then(@"the sidebar matches pixel by pixel")]
    public async Task ThenTheSidebarMatchesPixelByPixel()
    {
        // Note: Headless screenshots are a few pixels different to non-headless screenshots
        var screenshotName = _sideBarPageObject.Headless ? "sidebar_admin_headless.txt" : "sidebar_admin.txt";
        var saved = File.ReadAllText($"./Screenshots/{screenshotName}");

        var bytes = await _sideBarPageObject.Page.Locator(".mud-drawer-content").ScreenshotAsync();
        var current = Convert.ToBase64String(bytes);

        var matched = current == saved;
        if (!matched)
        {
            File.WriteAllText($"./Screenshots/latest_{screenshotName}", current);
        }

        matched.Should().BeTrue();
    }
}