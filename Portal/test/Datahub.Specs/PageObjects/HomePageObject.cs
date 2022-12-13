using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class HomePageObject : BasePageObject
{
    public override string PagePath => "https://localhost:5001/home";
    public sealed override IPage Page { get; set; }
    public sealed override IBrowser Browser { get; }

    public HomePageObject(IBrowser browser)
    {
        Browser = browser;
    }
}