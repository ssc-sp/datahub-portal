namespace Datahub.Portal.Pages;

public static class PageRoutes
{
    public const string Explore = "/explore";
    public const string Home = "/home";
    public const string News = "/news";
    
    public const string Achievements = "/achievements";
    
    public const string WorkspaceDefault = "/w/{WorkspaceAcronymParam}";
    public const string Workspace = "/w/{WorkspaceAcronymParam}/{Section}";
    
    public const string SettingsDefault = "/settings/";
    public const string Settings = "/settings/{Section}";
}