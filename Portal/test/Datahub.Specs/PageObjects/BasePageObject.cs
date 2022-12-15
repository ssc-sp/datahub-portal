using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public abstract class BasePageObject
{
    protected const string HomePath = "home";
    protected const string LoginPath = "login";

    public abstract string BaseUrl { get; }
    public abstract string PagePath { get; }

    public abstract IPage? Page { get; set; }

    public abstract IBrowser Browser { get; }

    public async Task NavigateAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions()
        {
            StorageStatePath = "auth.json"
        });
        
        Page = await context.NewPageAsync();
        await Page.GotoAsync($"{BaseUrl}/{PagePath}");
    }
}