using Datahub.Specs.PageObjects;
using Reqnroll;

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

    [Given(@"the user navigates to the profile page")]
    public async Task GivenTheUserNavigatesToTheProfilePage()
    {
        await _profilePageObject.NavigateAsync();
    }

    [Then(@"the profile page is reached")]
    public async Task ThenTheProfilePageIsReached()
    {
        await _profilePageObject.ValidateLocationAsync("profile");
    }

    [Given(@"click on the appearance settings button")]
    public async Task GivenClickOnTheAppearanceSettingsButton()
    {
        await _profilePageObject.ClickAppearanceSettingsButtonAsync();
    }

    [Then(@"the profile appearance settings page is reached")]
    public async Task ThenTheProfileAppearanceSettingsPageIsReached()
    {
        await _profilePageObject.ValidateLocationAsync("profile/settings/appearance");
    }

    [Given(@"click on the notification settings button")]
    public async Task GivenClickOnTheNotificationSettingsButton()
    {
        await _profilePageObject.ClickNotificationSettingsButtonAsync();
    }

    [Then(@"the profile notification settings page is reached")]
    public async Task ThenTheProfileNotificationSettingsPageIsReached()
    {
        await _profilePageObject.ValidateLocationAsync("profile/settings/notifications");
    }

    [Given(@"click on the view all achievements button")]
    public async Task GivenClickOnTheViewAllAchievementsButton()
    {
        await _profilePageObject.ClickAchievementsButtonAsync();
    }

    [Then(@"the profile achievements page is reached")]
    public async Task ThenTheProfileAchievementsPageIsReached()
    {
        await _profilePageObject.ValidateLocationAsync("profile/achievements");
    }
}