using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Playwright.Axe;

namespace Datahub.Specs.PageObjects;

public abstract class BasePageObject
{
    protected const string AuthStoragePath = "auth.json";
    private IPage _page = default!;

    public BasePageObject(IConfiguration configuration, IBrowser browser, string path)
    {
        Configuration = configuration;
        Browser = browser;
        Path = path;
    }

    protected readonly IConfiguration Configuration;
    public string BaseUrl => Configuration["BaseUrl"] ?? "https://localhost:5001";
    public IBrowser Browser { get; }
    public string Path { get; }

    public IPage Page 
    { 
        get
        {
            if (_page is null)
            {
                throw new Exception("Cannot access the 'Page' before navigation has been invoked!");
            }
            return _page;
        }
        set
        {
            _page = value;
        }
    }

    public async Task NavigateAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions()
        {
            StorageStatePath = AuthStoragePath
        });

        Page = await context.NewPageAsync();
        await Page.GotoAsync($"{BaseUrl}/{Path}");
    }

    public async Task<AxeResults> RunAxe()
    {
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        return await Page.RunAxe();
    }
}
