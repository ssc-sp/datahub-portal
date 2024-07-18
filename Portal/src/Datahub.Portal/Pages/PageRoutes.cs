using Datahub.Application.Services.ReverseProxy;

namespace Datahub.Portal.Pages;

public static class PageRoutes
{
    public const string Explore = "/explore";
    public const string Home = "/home";
    public const string News = "/news";
    
    public const string WorkspacePrefix = "w";
    public const string WorkspaceDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}";
    public const string Workspace = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}";
    public const string WorkspaceSubSection = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}/{{SubSection}}";

    public const string WorkspaceWebAppShare = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/webapp-ext";

    public const string AccountPrefix = "account";
    public const string AccountDefault = $"/{AccountPrefix}/";
    public const string Account = $"/{AccountPrefix}/{{Section}}";
    
    public const string ResourcePrefix = "resources";
    public const string ResourceDefault = $"/{ResourcePrefix}/";
    public const string Resource = $"/{ResourcePrefix}/{{PageName}}";
    
    public const string ToolPrefix = "tool";
    public const string ToolDefault = $"/{ToolPrefix}/";
    public const string Tool = $"/{ToolPrefix}/{{Section}}";
    
    public const string WebAppDefault = $"/{IReverseProxyConfigService.WebAppPrefix}/";
    public const string WebApp = $"/{WorkspacePrefix}/{IReverseProxyConfigService.WebAppPrefix}/{{Section}}";

    public const string Logout = "/signout-oidc";
    public const string TermsAndConditions = "/terms-and-conditions";

    public const string Help = "/help";
}