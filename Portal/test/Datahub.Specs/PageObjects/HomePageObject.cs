using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class HomePageObject : BasePageObject
{
    public HomePageObject(IConfiguration configuration, IBrowser browser) : base(configuration, browser, true, path: "home")
    {
    }
}
