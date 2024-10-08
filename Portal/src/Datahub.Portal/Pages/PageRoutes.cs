using Datahub.Application.Services.ReverseProxy;

namespace Datahub.Portal.Pages;

public static class PageRoutes
{
    public const string Explore = "/explore";
    public const string Explore_FR = "/explorer";
    public const string Home = "/home";
    public const string Home_FR = "/accueil";
    public const string News = "/announcements";
    public const string News_FR = "/annonces";

    public const string WorkspacePrefix = "w";
    public const string WorkspaceDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}";
    public const string Workspace = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}";
    public const string WorkspaceSubSection = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}/{{SubSection}}";

    public const string WorkspaceWebAppShare = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/app-ext";

    public const string AccountPrefix = "account";
    public const string AccountPrefix_FR = "compte";
    public const string AccountDefault = $"/{AccountPrefix}/";
    public const string AccountDefault_FR = $"/{AccountPrefix_FR}/";
    public const string Account = $"/{AccountPrefix}/{{Section}}";
    public const string Account_FR = $"/{AccountPrefix_FR}/{{Section}}";
    
    public const string ResourcePrefix = "resources";
    public const string ResourcePrefix_FR = "ressources";
    public const string ResourceDefault = $"/{ResourcePrefix}/";
    public const string ResourceDefault_FR = $"/{ResourcePrefix_FR}/";
    public const string Resource = $"/{ResourcePrefix}/{{PageName}}";
    public const string Resource_FR = $"/{ResourcePrefix_FR}/{{PageName}}";
    
    public const string ToolPrefix = "tool";
    public const string ToolPrefix_FR = "outil";
    public const string ToolDefault = $"/{ToolPrefix}/";
    public const string ToolDefault_FR = $"/{ToolPrefix_FR}/";
    public const string Tool = $"/{ToolPrefix}/{{Section}}";
    public const string Tool_FR = $"/{ToolPrefix_FR}/{{Section}}";
    
    public const string Logout = "/signout-oidc";
    public const string TermsAndConditions = "/terms-and-conditions";
    public const string TermsAndConditions_FR = "/conditions-generales";
    
    public const string LanguageSelection_Bilingual = "/language-langue";

    public const string Help = "/support";
    public const string Help_FR = "/assistance";
  
    public const string CreateWorkspace = "/create-workspace";
    public const string CreateWorkspace_FR = "/creer-espace-de-travail";
}