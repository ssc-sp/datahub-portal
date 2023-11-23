namespace Datahub.Portal.Pages;

public static class PageRoutes
{
    public const string Explore = "/explore";
    public const string Home = "/home";
    public const string News = "/news";
    
    public const string Achievements = "/achievements";
    
    public const string WorkspacePrefix = "w";
    public const string WorkspaceDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}";
    public const string Workspace = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}";
    
    public const string AccountPrefix = "account";
    public const string AccountDefault = $"/{AccountPrefix}/";
    public const string Account = $"/{AccountPrefix}/{{Section}}";
}