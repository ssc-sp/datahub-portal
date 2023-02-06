using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class WorkspacePageObject : BasePageObject
{
    public WorkspacePageObject(IConfiguration configuration, IBrowser browser) : base(configuration, browser, path: "w")
    {
    }
}
