using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class LoginPageObject : BasePageObject
{
    private readonly IConfiguration _configuration;
    
    public override string BaseUrl => _configuration["BaseUrl"];
    public override string PagePath => LoginPath;
    public sealed override IPage Page { get; set; }
    public sealed override IBrowser Browser { get; }

    public LoginPageObject(IBrowser browser, IConfiguration configuration)
    {
        Browser = browser;
        _configuration = configuration;
    }

    public async Task LoginAsync()
    {
        await NavigateAsync();
        
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Log in" })
            .ClickAsync();
        
        if (Page.Url.EndsWith("home"))
        {
            return;
        }
        
        await Page.GetByPlaceholder("Email, phone, or Skype")
            .FillAsync(_configuration["Username"]);
        
        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Next" })
            .ClickAsync();
        
        await Page.Locator("#i0118")
            .FillAsync(_configuration["Password"]);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Sign in" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "No" })
            .ClickAsync();

        await Page.Context.StorageStateAsync(new BrowserContextStorageStateOptions
        {
            Path = "auth.json"
        });
    }
}