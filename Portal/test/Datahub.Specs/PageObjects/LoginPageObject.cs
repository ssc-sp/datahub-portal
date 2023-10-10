using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class LoginPageObject : BasePageObject
{
    public LoginPageObject(IConfiguration configuration, IBrowser browser, bool admin) 
        : base(configuration, browser, admin, path: "login")
    {
    }

    public async Task LoginAsync(bool cleanAuthCache = false)
    {
        if (cleanAuthCache)
            DoCleanAuthCache();

        await NavigateAsync();

        if (Page!.Url.EndsWith("home"))
            return;

        // click the new Login button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Enter username
        await Page.FillAsync("input[name='loginfmt']", GetUserName());
        await Page.ClickAsync("input[type=submit]");

        // Wait until page has changed and is loaded
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Enter password
        await Page.FillAsync("input[name='passwd']", GetPassword());
        await Page.ClickAsync("input[type=submit]");

        // Wait until page has changed and is loaded
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "No" }).ClickAsync();

        // Wait until page has changed and is loaded
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // create authentication context
        await Page.Context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = AuthStoragePath
        });
    }

    private void DoCleanAuthCache()
    {
        if (File.Exists(AuthStoragePath))
        {
            try
            {
                File.Delete(AuthStoragePath);
            }
            catch 
            { 
            }
        }
    }

    private string GetUserName() => Configuration[Admin ? "admin_user" : "user_name"]!;
    private string GetPassword() => Configuration[Admin ? "admin_password" : "password"]!;
}