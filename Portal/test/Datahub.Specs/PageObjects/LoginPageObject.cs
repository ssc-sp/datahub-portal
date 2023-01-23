using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class LoginPageObject : BasePageObject
{
    private readonly IConfiguration _configuration;
    
    public override string BaseUrl => _configuration["BaseUrl"]!;
    public override string PagePath => LoginPath;
    public sealed override IPage Page { get; set; }
    public sealed override IBrowser Browser { get; }

    public LoginPageObject(IBrowser browser, IConfiguration configuration)
    {
        Browser = browser;
        _configuration = configuration;
        Page = null!;
    }

    public async Task LoginAsync()
    {
        await NavigateAsync();

        if (Page!.Url.EndsWith("home") || Page!.Url.EndsWith("login"))
            return;

        // Enter username
        await Page.FillAsync("input[name='loginfmt']", _configuration["Username"]!);
        await Page.ClickAsync("input[type=submit]");

        // Wait until page has changed and is loaded
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Enter password
        await Page.FillAsync("input[name='passwd']", _configuration["password"]!);
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
}