using Datahub.Specs.PageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;

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
        string? scriptRun = Environment.GetEnvironmentVariable("SCRIPT_RUN");
        if (scriptRun != null)
        {
            _config["BaseUrl"] = Environment.GetEnvironmentVariable("TEST_URL");
            _config["Headless"] = Environment.GetEnvironmentVariable("HEADLESS");
            _config["SlowMo"] = Environment.GetEnvironmentVariable("SLOWMO");
        }
    }

    public bool Headless => _config["Headless"] != "false";

    #region Scenario utils

    static LoginPageObject loginPageObject = default!;
    static LoginPageObject adminLoginPageObject = default!;

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
                loginPageObject = new(_config, browser, false);
                mustLogin = true;
            }
            container.RegisterInstanceAs(loginPageObject);
        }

        if (mustLogin)
        {
            // always start scenario after login completed
            await loginPageObject.LoginAsync(true);
        }

        mustLogin = false;
        lock (typeof(Hooks))
        {
            if (adminLoginPageObject is null)
            {
                // register the login page
                adminLoginPageObject = new(_config, browser, true);
                mustLogin = true;
            }
        }

        if (mustLogin)
        {
            await adminLoginPageObject.LoginAsync(true);
        }
    }

    private BrowserTypeLaunchOptions GetBrowserOptions()
    {
        var options = new BrowserTypeLaunchOptions() 
        { 
            Headless = this.Headless
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

    [BeforeScenario("home")]
    public async Task BeforeHomeScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser)
            => container.RegisterInstanceAs(new HomePageObject(config, browser)));
    }

    [BeforeScenario("ws")]
    public async Task BeforeWsScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) => 
        {
            container.RegisterInstanceAs(new WorkspacePageObject(new(), config, browser));
            container.RegisterInstanceAs(new WorkspaceAdminPageObject(new(), config, browser));
        });
    }

    [BeforeScenario("res")]
    public async Task BeforeResScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) => 
        {
            container.RegisterInstanceAs(new ResourcesPageObject(config, browser));
        });
    }

    [BeforeScenario("news")]
    public async Task BeforeNewsScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) =>
        {
            container.RegisterInstanceAs(new NewsPageObject(config, browser));
        });
    }

    [BeforeScenario("admnews")]
    public async Task BeforeAdminNewsScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) =>
        {
            container.RegisterInstanceAs(new NewsPageObjectAdmin(config, browser));
        });
    }

    [BeforeScenario("admintools")]
    public async Task BeforeAdminToolsScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) =>
        {
            container.RegisterInstanceAs(new AdminToolsPageObject(config, browser));
        });
    }

    [BeforeScenario("prof")]
    public async Task BeforeProfileScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) =>
        {
            container.RegisterInstanceAs(new ProfilePageObject(config, browser));
        });
    }

    [BeforeScenario("sidebar")]
    public async Task BeforeSidebarScenario(IObjectContainer container)
    {
        await SetupScenario(container, (config, browser) =>
        {
            container.RegisterInstanceAs(new SideBarPageObject(config, browser, Headless));
        });
    }

    [AfterScenario("home", "a11y", "ws", "res", "news", "admnews", "admintools", "prof", "sidebar")]
    public static Task AfterResScenario(IObjectContainer container)
    {
        return TeardownScenario(container);
    }
}