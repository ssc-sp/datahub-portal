using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class ResourcesPageObject : BasePageObject
{
    public ResourcesPageObject(IConfiguration configuration, IBrowser browser) : base(configuration, browser, path: "resources")
    {
    }
}
