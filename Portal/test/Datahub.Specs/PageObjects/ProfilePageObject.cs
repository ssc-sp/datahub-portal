using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class ProfilePageObject : BasePageObject
{
    public ProfilePageObject(IConfiguration configuration, IBrowser browser) : base(configuration, browser, false, path: "profile")
    {
    }

    public async Task ClickProfileSettingsButtonAsync()
    {
        await Page.GetByRole(AriaRole.Link, new() { Name = "Manage Settings" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickAppearanceSettingsButtonAsync()
    {
        await Page.GetByRole(AriaRole.Link, new () {Name = "Appearance"}).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    public async Task ClickNotificationSettingsButtonAsync()
    {
        await Page.GetByRole(AriaRole.Link, new () {Name = "Notifications"}).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClickAchievementsButtonAsync()
    {
        await Page.GetByRole(AriaRole.Link, new () {Name = "View All Achievements"}).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
