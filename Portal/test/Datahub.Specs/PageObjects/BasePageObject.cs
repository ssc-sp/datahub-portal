using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Playwright.Axe;

namespace Datahub.Specs.PageObjects;

public abstract class BasePageObject
{
    const string ErrorMarkerId = "datahub-error-page";

    private IPage _page = default!;

    public BasePageObject(IConfiguration configuration, IBrowser browser, bool admin, string path)
    {
        Configuration = configuration;
        Browser = browser;
        Admin = admin;
        Path = path;
    }

    protected bool Admin { get; }
    protected string AuthStoragePath => Admin ? "admin_auth.json" : "user_auth.json";

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
        var stgStatePath = File.Exists(AuthStoragePath) ? AuthStoragePath : string.Empty;

        if (_page is null)
        {
            var context = await Browser.NewContextAsync(new BrowserNewContextOptions()
            {
                StorageStatePath = stgStatePath,
                DeviceScaleFactor = 1.0f,
                ViewportSize = new() { Width = 1440, Height = 800 }
            });
            _page = await context.NewPageAsync();
        }

        await _page.GotoAsync($"{BaseUrl}/{Path}");
    }

    public async Task<AxeResults> RunAxe()
    {
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        return await Page.RunAxe();
    }

    public async Task<bool> IsErrorPage()
    {
        var locator = await Page.QuerySelectorAsync(ErrorMarkerId);
        return locator is not null;
    }

    public async Task ValidateLocationAsync(string? expectedPath = null)
    {
        Page.Url.Should().EndWith($"/{expectedPath ?? Path}");
        (await IsErrorPage()).Should().BeFalse();
    }
}
