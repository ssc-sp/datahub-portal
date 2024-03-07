namespace Datahub.Portal.Pages;

public static class PageRoutes
{
    public const string Explore = "/explore";
    public const string Home = "/home";
    public const string News = "/news";

    public const string WorkspacePrefix = "w";
    public const string WorkspaceDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}";
    public const string Workspace = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}";

    public const string AccountPrefix = "account";
    public const string AccountDefault = $"/{AccountPrefix}/";
    public const string Account = $"/{AccountPrefix}/{{Section}}";

    public const string ResourcePrefix = "resources";
    public const string ResourceDefault = $"/{ResourcePrefix}/";
    public const string Resource = $"/{ResourcePrefix}/{{PageName}}";

    public const string ToolPrefix = "tool";
    public const string ToolDefault = $"/{ToolPrefix}/";
    public const string Tool = $"/{ToolPrefix}/{{Section}}";

    public const string WebAppPrefix = "webapp";
    public const string WebAppDefault = $"/{WebAppPrefix}/";
    public const string WebApp = $"/{WorkspacePrefix}/{WebAppPrefix}/{{Section}}";

    public const string Logout = "/signout-oidc";
    public const string TermsAndConditions = "/terms-and-conditions";
}