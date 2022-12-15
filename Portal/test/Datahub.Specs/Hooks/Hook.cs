using BoDi;
using Datahub.Specs.PageObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("a11y")]
    public async Task BeforeA11yScenario(IObjectContainer container)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.secret.json")
            .Build();
        
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            // Headless = false,
            // SlowMo = 2000
        });
        
        var loginPageObject = new LoginPageObject(browser, config);
        var homePageObject = new HomePageObject(browser, config);
        
        container.RegisterInstanceAs(playwright);
        container.RegisterInstanceAs(browser);
        container.RegisterInstanceAs(loginPageObject);
        container.RegisterInstanceAs(homePageObject);
    }

    [AfterScenario("a11y")]
    public async Task AfterA11yScenario(IObjectContainer container)
    {
        var browser = container.Resolve<Microsoft.Playwright.IBrowser>();
        await browser.CloseAsync();

        var playwright = container.Resolve<Microsoft.Playwright.IPlaywright>();
        playwright.Dispose();
    }
}