using Datahub.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace Datahub.Specs.PageObjects;

public class WorkspacePageObject : BasePageObject
{
    public WorkspacePageObject(DatahubPortalConfiguration datahubPortalConfiguration, IConfiguration configuration, IBrowser browser) 
        : base(configuration, browser, false, path: datahubPortalConfiguration.ProjectUrlSegment)
    {
    }
}

public class WorkspaceAdminPageObject : BasePageObject
{
    public WorkspaceAdminPageObject(DatahubPortalConfiguration datahubPortalConfiguration, IConfiguration configuration, IBrowser browser)
        : base(configuration, browser, true, path: datahubPortalConfiguration.ProjectUrlSegment)
    {
    }
}
