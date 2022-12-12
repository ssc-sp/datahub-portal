using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class HomePageObject : BasePageObject
{
    public override string PagePath => "https://localhost:5001/home";
    public override IPage Page { get; set; }
    public override IBrowser Browser { get; }

    public HomePageObject(IBrowser browser)
    {
        Browser = browser;
    }
}