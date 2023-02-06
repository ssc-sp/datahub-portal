using BoDi;
using Datahub.Specs.PageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.Hooks;

[Binding]
public class Hooks
{
    private readonly IConfiguration _config;

    public Hooks()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.secret.json")
            .AddUserSecrets<Hooks>()
            .Build();
    }

    #region Scenario utils

    static LoginPageObject loginPageObject = default!;

    private async Task SetupScenario(IObjectContainer container, Action<IConfiguration, IBrowser> setup)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(GetBrowserOptions());

        // callback scenario
        setup.Invoke(_config, browser);

        container.RegisterInstanceAs(playwright);
        container.RegisterInstanceAs(browser);

        var mustLogin = false;
        lock (typeof(Hooks))
        {
            if (loginPageObject is null)
            {
                // register the login page
                loginPageObject = new(_config, browser);
                mustLogin = true;
            }
            container.RegisterInstanceAs(loginPageObject);
        }

        if (mustLogin)
        {
            // always start scenario after login completed
            await loginPageObject.LoginAsync();
        }
    }

    private BrowserTypeLaunchOptions GetBrowserOptions()
    {
        var options = new BrowserTypeLaunchOptions() 
        { 
            Headless = _config["Headless"] != "false"
        };
        if (int.TryParse(_config["SlowMo"], out int slowmo))
        {
            options.SlowMo = slowmo;
        }
        return options;
    }

    private static async Task TeardownScenario(IObjectContainer container)
    {
        var browser = container.Resolve<Microsoft.Playwright.IBrowser>();
        await browser.CloseAsync();

        var playwright = container.Resolve<Microsoft.Playwright.IPlaywright>();
        playwright.Dispose();
    }

    #endregion

    [BeforeScenario("a11y")]
    public async Task BeforeA11yScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) 
            => container.RegisterInstanceAs(new HomePageObject(config, browser)));
    }

    [BeforeScenario("ws")]
    public async Task BeforeWsScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser)
            => container.RegisterInstanceAs(new WorkspacePageObject(config, browser)));
    }

    [BeforeScenario("res")]
    public async Task BeforeResScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser)
            => container.RegisterInstanceAs(new ResourcesPageObject(config, browser)));
    }

    [AfterScenario("a11y", "ws", "res")]
    public static Task AfterResScenario(IObjectContainer container)
    {
        return TeardownScenario(container);
    }
}