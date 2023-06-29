using Datahub.Specs.PageObjects;
using Microsoft.Playwright;

namespace Datahub.Specs.Steps;

[Binding]
// ReSharper disable once InconsistentNaming
public sealed class ProfileStepDefinitions
{

    private readonly ProfilePageObject _profilePageObject;

    public ProfileStepDefinitions(ProfilePageObject homePageObject)
    {
        _profilePageObject = homePageObject;
    }

    [Given(@"the user is on the profile page")]
    public async Task GivenTheUserIsOnTheProfilePage()
    {
        await _profilePageObject.NavigateAsync();
        await _profilePageObject.ValidateLocationAsync();

        //await _profilePageObject.Page.ScreenshotAsync(new()
        //{
        //    Path = @"c:\temp\playwright\screenshot.png",
        //});

        //await _profilePageObject.Page.Locator(".mud-drawer-content").ScreenshotAsync(new() 
        //{
        //    Path = @"c:\temp\playwright\sidebar.png",
        //});

        // mud-drawer-content
    }

    [Given(@"click on the profile settings button")]
    public async Task GivenClickOnTheProfileSettingsButton()
    {
        await _profilePageObject.ClickProfileSettingsButtonAsync();
    }

    [Then(@"the profile settings page is reached")]
    public async Task ThenTheProfileSettingsPageIsReached()
    {
        await _profilePageObject.ValidateLocationAsync("profile/settings/public");
    }
}