using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class SideBarPageObject : BasePageObject
{
    public SideBarPageObject(IConfiguration configuration, IBrowser browser, bool headless) : base(configuration, browser, true, "home")
    {
        Headless = headless;
    }

    public bool Headless { get; }
}
