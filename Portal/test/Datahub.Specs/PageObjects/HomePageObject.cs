using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class HomePageObject : BasePageObject
{
    private readonly IConfiguration _configuration;
    public override string BaseUrl => _configuration["BaseUrl"];
    public override string PagePath => HomePath;
    public sealed override IPage Page { get; set; }
    public sealed override IBrowser Browser { get; }

    public HomePageObject(IBrowser browser, IConfiguration configuration)
    {
        Browser = browser;
        _configuration = configuration;
    }
}