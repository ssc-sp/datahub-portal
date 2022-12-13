using BoDi;
using Datahub.Specs.PageObjects;
using Microsoft.Playwright;

namespace Datahub.Specs.Hooks;

[Binding]
public class Hooks
{
    [BeforeScenario("a11y")]
    public async Task BeforeA11yScenario(IObjectContainer container)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 5000
        });
        
        var pageObject = new HomePageObject(browser);
        await pageObject.Page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
        {
        });

        container.RegisterInstanceAs(playwright);
        container.RegisterInstanceAs(browser);
        container.RegisterInstanceAs(pageObject);
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