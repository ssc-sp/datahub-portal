using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using FluentAssertions;

namespace Datahub.Specs.PageObjects;

public class NewsPageObject : BasePageObject
{
    public NewsPageObject(IConfiguration configuration, IBrowser browser) : base(configuration, browser, false, path: "news")
    {
    }
}

public class NewsPageObjectAdmin : BasePageObject
{
    public NewsPageObjectAdmin(IConfiguration configuration, IBrowser browser) : base(configuration, browser, true, path: "news")
    {
    }

    public async Task AssertCreateNewsButtonExistsAsync()
    {
        var link = await Page.QuerySelectorAsync("a[href='/news/edit/new']");
        link.Should().NotBeNull();
    }

    public async Task ClickCreateNewsButtonAsync()
    {
        var link = await Page.QuerySelectorAsync("a[href='/news/edit/new']");
        await link!.ClickAsync();

        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
