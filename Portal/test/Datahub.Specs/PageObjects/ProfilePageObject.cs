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
}
