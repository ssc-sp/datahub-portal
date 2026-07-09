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

    public const string GCCF_Login = "/gccf/login";

    public const string WorkspacePrefix = "w";
    public const string WorkspaceDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}";
    public const string Workspace = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}";
    public const string WorkspaceSubSection = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{{Section}}/{{SubSection}}";

    public const string WorkspaceWebAppShare = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/app-ext";

    public const string WorkspaceStoragePrefix = "s";
    public const string WorkspaceStorageDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{WorkspaceStoragePrefix}";

    public const string WorkspaceExternalStoragePrefix = "s-ext";
    public const string WorkspaceExternalStorageDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{WorkspaceExternalStoragePrefix}";

    public const string WorkspacePostgresPrefix = "db";
    public const string WorkspacePostgresDefault = $"/{WorkspacePrefix}/{{WorkspaceAcronymParam}}/{WorkspacePostgresPrefix}";

    public const string AccountPrefix = "account";
    public const string AccountPrefix_FR = "compte";
    public const string AccountDefault = $"/{AccountPrefix}";
    public const string AccountDefault_FR = $"/{AccountPrefix_FR}";
    public const string Account = $"/{AccountPrefix}/{{Section}}";
    public const string Account_FR = $"/{AccountPrefix_FR}/{{Section}}";
    
    public const string ResourcePrefix = "resources";
    public const string ResourcePrefix_FR = "ressources";
    public const string ResourceDefault = $"/{ResourcePrefix}/";
    public const string ResourceDefault_FR = $"/{ResourcePrefix_FR}/";
    public const string Resource = $"/{ResourcePrefix}/{{PageName}}";
    public const string Resource_FR = $"/{ResourcePrefix_FR}/{{PageName}}";
    public const string ResourceSite = "https://documentation.sds.canada.ca/";
    public const string ResourceSite_Preregistration = "https://documentation.sds.canada.ca/en/managing-workspaces-and-users/Preregistration.html";
    public const string ResourceSite_AddUserPostgres = "https://documentation.sds.canada.ca/en/postgresql/Postgres-Add-User.html";
    public const string ResourceSite_UseAzCopy = "https://documentation.sds.canada.ca/en/storage/Use-AzCopy.html";
    public const string ResourceSite_DesktopUploader = "https://documentation.sds.canada.ca/en/storage/Desktop-Uploader.html";
    public const string ResourceSite_ImportAWS = "https://documentation.sds.canada.ca/en/storage/Import-AWS-Storage.html";
    public const string ResourceSite_ImportAzure = "https://documentation.sds.canada.ca/en/storage/Import-Azure-Storage.html";
    public const string ResourceSite_ImportGCP = "https://documentation.sds.canada.ca/en/storage/Import-GCP-Storage.html";
    public const string ResourceSite_Import = "https://documentation.sds.canada.ca/en/storage/Import-Storage.html";


    public const string ToolPrefix = "tool";
    public const string ToolPrefix_FR = "outil";
    public const string ToolDefault = $"/{ToolPrefix}/";
    public const string ToolDefault_FR = $"/{ToolPrefix_FR}/";
    public const string Tool = $"/{ToolPrefix}/{{Section}}";
    public const string Tool_FR = $"/{ToolPrefix_FR}/{{Section}}";

    public const string NotFound = "/404";

    public const string Logout = "/signout-oidc";
    public const string Login = "/login";
    public const string Login_FR = "/connexion";
    public const string LoginEntra = "/login-entra";
    public const string LoginEntra_FR = "/connexion-entra";
    public const string TermsAndConditions = "https://documentation.sds.canada.ca/en/Terms-And-Conditions.html";
    public const string TermsAndConditions_FR = "https://documentation.sds.canada.ca/fr/Conditions-generales.html";

    public const string TermsAndConditionsExternal = "/external-terms-and-conditions";
    public const string TermsAndConditionsExternal_FR = "/conditions-generales-externes";

    public const string LanguageSelection_Bilingual = "/language-langue";

    public const string Help = $"/support";
    public const string Help_FR = $"/assistance";
    public const string HelpWithParam = $"/support/{{CorrelationId}}/";
    public const string HelpWithParam_FR = $"/assistance/{{CorrelationId}}/";

    public const string CreateWorkspace = "/create-workspace";
    public const string CreateWorkspace_FR = "/creer-espace-de-travail";

    public const string AccessDenied = "/access-denied";
    public const string AccessDenied_FR = "/acces-refuse";

    public const string ExternalInvitationSetup = "/account-setup";
    public const string ExternalInvitationSetupWithToken = $"{ExternalInvitationSetup}/{{InvitationToken:guid}}";

    public const string ExternalInvitationSetup_FR = "/configuration-compte";
    public const string ExternalInvitationSetupWithToken_FR = $"{ExternalInvitationSetup_FR}/{{InvitationToken:guid}}";

    public const string ExternalLogoutConfirmation = "/logout-confirmation";
    public const string ExternalLogoutConfirmation_FR = "/confirmation-de-deconnexion";
}
