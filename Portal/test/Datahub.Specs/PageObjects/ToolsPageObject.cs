using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class ToolsPageObject : BasePageObject
{
    public ToolsPageObject(IConfiguration configuration, IBrowser browser) 
        : base(configuration, browser, false, path: "tools")
    {
    }
}

public class AdminToolsPageObject : BasePageObject
{
    public AdminToolsPageObject(IConfiguration configuration, IBrowser browser)
        : base(configuration, browser, true, path: "tools")
    {
    }

    public async Task AssertToolsButtonExistsAsync()
    {
        var link = await Page.QuerySelectorAsync("span.mud-icon-root.mud-icon-size-medium.fad.fa-tools");
        link.Should().NotBeNull();
    }

}

