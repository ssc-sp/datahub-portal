using BoDi;
using Datahub.Specs.PageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.ComponentModel;

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

    private async Task SetupScenario(IObjectContainer container, Action<IBrowser, IConfiguration> setup, bool headless = true, int? slowmo = default)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        BrowserTypeLaunchOptions browserOptions = new() { Headless = headless };
        if (slowmo.HasValue)
        {
            browserOptions.SlowMo = slowmo.Value;
        }

        var browser = await playwright.Chromium.LaunchAsync(browserOptions);

        setup.Invoke(browser, _config);

        container.RegisterInstanceAs(playwright);
        container.RegisterInstanceAs(browser);
    }

    private async Task TeardownScenario(IObjectContainer container)
    {
        var browser = container.Resolve<Microsoft.Playwright.IBrowser>();
        await browser.CloseAsync();

        var playwright = container.Resolve<Microsoft.Playwright.IPlaywright>();
        playwright.Dispose();
    }

    [BeforeScenario("a11y")]
    public async Task BeforeA11yScenario(IObjectContainer container)
    {
        LoginPageObject loginPageObject = null!;

        await SetupScenario(container, (browser, config) =>
        {
            container.RegisterInstanceAs(loginPageObject = new LoginPageObject(browser, config));
            container.RegisterInstanceAs(new HomePageObject(browser, config));
        });

        await loginPageObject.NavigateAsync();
    }

    [AfterScenario("a11y")]
    public Task AfterA11yScenario(IObjectContainer container) => TeardownScenario(container);
}