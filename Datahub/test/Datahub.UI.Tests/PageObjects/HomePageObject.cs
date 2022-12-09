using Microsoft.Playwright;

namespace Datahub.UI.Tests
{
    
    public class HomePageObject: BasePageObject
    {
        public override string PagePath => "localhost:5001/home";

        public HomePageObject(IBrowser browser) 
        {
            Browser = browser;
        }

        public override IPage Page { get; set; }

        public override IBrowser Browser { get; }

    }
}