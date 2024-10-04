using Newtonsoft.Json;

namespace Datahub.Application.Configuration;

public class DatahubPortalConfiguration
{
    public CultureSettings CultureSettings { get; set; } = new();
    public string LandingBgFolder { get; set; } = "ssc";
    public int LandingBgImgCount { get; set; } = 2;
    public bool ShowLoginPage { get; set; } = true;
    public string Title { get; set; } = "Federal Science DataHub default title";
    public string DataHubModules { get; set; } = null!;
    public DataProjects DataProjects { get; set; } = new();
    public Achievements Achievements { get; set; } = new();
    public Media Media { get; set; } = new();
    public string ProjectUrlSegment { get; set; } = "w";
    public string ProfileUrlSegment { get; set; } = "profile";

    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public AzureAd AzureAd { get; set; } = new();
    public AdoServiceUser AdoServiceUser { get; set; } = new();
    public AdoOrg AdoOrg { get; set; } = new();
    public Graph Graph { get; set; } = new();
    public APITargets APITargets { get; set; } = new();
    public ApplicationInsights ApplicationInsights { get; set; } = new();
    public KeyVault KeyVault { get; set; } = new();
    public Hosting Hosting { get; set; } = new();
    public EmailNotification EmailNotification { get; set; } = new();
    public PublicFileSharing PublicFileSharing { get; set; } = new();
    public string PortalRunAsManagedIdentity { get; set; } = "disabled";
    public string ResourcePrefix { get; set; } = "fsdh";
    public bool CentralizedProjectSecrets { get; set; } = false;
    public string ProjectStorageKeySecretName { get; set; } = "storage-key";

    [JsonProperty("Azure:SignalR:StickyServerMode")]
    public string AzureSignalRStickyServerMode { get; set; } = "Required";

    public string AllowedHosts { get; set; } = null!;
    public string[] AllowedUserEmailDomains { get; set; } = [".gc.ca"];
    public string DatahubGraphInviteFunctionUrl { get; set; } = null!;
    public string DatahubGraphUsersStatusFunctionUrl { get; set; } = null!;
    public TermsAndConditionsUrl TermsAndConditionsUrl { get; set; } = new();
    public PreRegistrationDocumentationUrl PreRegistrationDocumentationUrl { get; set; } = new();

    public string SupportFormUrl { get; set; } =
        "https://forms.office.com/pages/responsepage.aspx?id=lMFb0L-U1kquLh2w8uOPXhksOXzZ73RCp9fVTz4vTU5UNTc1U00yNVUxWVg4SkJGMFVHN1RCTTdQRS4u";

    public ReverseProxy ReverseProxy { get; set; } = new();

    public GithubConfig Github { get; set; } = new();

    public CkanConfiguration CkanConfiguration { get; set; } = new();
    public int DefaultProjectBudget { get; set; } = 100;
    public DatahubServiceBus DatahubServiceBus { get; set; } = new();
}

public class Achievements
{
    public bool Enabled { get; set; } = false;
    public bool LocalAchievementsOnly { get; set; } = false;

}

public class Media
{
    public string StorageConnectionString { get; set; } = null!;
    public string StaticAssetsUrl { get; set; } = "https://fsdhstaticassetstorage.blob.core.windows.net";

    public string GetAchievementThumbnailUrl(string? code) => $"{StaticAssetsUrl}/achievements/thumbnails/{(string.IsNullOrWhiteSpace(code) ? "DHA-001" : code)}.jpg";
    public string GetAchievementPortraitUrl(string? code) => $"{StaticAssetsUrl}/achievements/portraits/{(string.IsNullOrWhiteSpace(code) ? "DHA-001" : code)}.jpg";
    public string GetAchievementImageUrl(string? code) => $"{StaticAssetsUrl}/achievements/backgrounds/{(string.IsNullOrWhiteSpace(code) ? "DHA-001" : code)}.jpg";
}


public class TermsAndConditionsUrl
{
    public string En { get; set; } =
        "https://raw.githubusercontent.com/ssc-sp/datahub-docs/main/UserGuide/POC-Terms-And-Conditions.md";

    public string Fr { get; set; } =
        "https://raw.githubusercontent.com/ssc-sp/datahub-docs/main/fr/UserGuide/Conditions-g%C3%A9n%C3%A9rales-de-POC.md";
}

public class PreRegistrationDocumentationUrl
{
    public string En { get; set; } =
        "/UserGuide/Preregistration/Preregistration.md";

    public string Fr { get; set; } =
        "/fr/UserGuide/Preregistration/Preregistration.md";
}

public class CultureSettings
{
    public string Default { get; set; } = "en-CA";
    public string SupportedCultures { get; set; } = "fr-CA|en-CA";
    public bool TrackTranslations = false;
    public string ResourcesPath { get; set; } = "../Datahub.Portal/i18n";
    public string[] AdditionalResourcePaths { get; set; } = ["../Datahub.Portal/i18n/ssc"];
}

public class DataProjects
{
    public bool PowerBI { get; set; } = false;
    public bool PublicSharing { get; set; } = false;
    public bool WebForms { get; set; } = false;
    public bool Databricks { get; set; } = true;
    public bool SQLServer { get; set; } = false;
    public bool PostgreSQL { get; set; } = false;
    public bool Costing { get; set; } = false;
    public bool Storage { get; set; } = true;
}

public class APITargets
{
    public string StorageURL { get; set; } = null!;
    public string SearchServiceName { get; set; } = null!;
    public string StorageAccountName { get; set; } = null!;
    public string KeyVaultName { get; set; } = null!;
    public string FileSystemName { get; set; } = null!;
    public string FileIndexName { get; set; } = null!;
    public string FileIndexerName { get; set; } = null!;
    public string LogoutURL { get; set; } = null!;
    public string LoginUrl { get; set; } = null!;
}

public class ApplicationInsights
{
    public string InstrumentationKey { get; set; } = null!;
}

public class AzureAd
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Domain { get; set; } = null!;
    public string TenantId { get; set; } = null!;
    public string SubscriptionId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string CallbackPath { get; set; } = "/signin-oidc";

    [JsonProperty("SignedOutCallbackPath ")]
    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    public string AppIDURL { get; set; } = null!;

    public string InfraClientId { get; set; } = null!;
    public string InfraClientSecret { get; set; } = null!;
}

public class AdoServiceUser
{
    public string OidSecretName { get; set; } = "ado-service-user-oid";
    public string PatSecretName { get; set; } = "ado-service-user-pat";
}

public class AdoOrg
{
    public string OrgName { get; set; } = "DataSolutionsDonnees";
    public string ProjectName { get; set; } = "FSDH SSC";
}

public class ConnectionStrings
{
    [JsonProperty("datahub_mssql_project")]
    public string? DatahubMsSqlProject { get; set; }

    [JsonProperty("datahub_mssql_pip")] public string? DatahubMsSqlPip { get; set; }

    [JsonProperty("datahub_mssql_etldb")] public string? DatahubMsSqlEtldb { get; set; }

    [JsonProperty("datahub_mssql_finance")]
    public string? DatahubMsSqlFinance { get; set; }

    [JsonProperty("datahub_mssql_webAnalytics")]
    public string? DatahubMsSqlWebAnalytics { get; set; }

    [JsonProperty("datahub_mssql_metadata")]
    public string? DatahubMsSqlMetadata { get; set; }

    // [JsonProperty("DATAHUB_MSSQL_LANGUAGETRAINING")]
    // public string? DatahubMsSqlLanguageTraining { get; set; }
}

public class EmailNotification
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "Placeholder Username";
    public string SmtpPassword { get; set; } = "Placeholder Password";
    public string SenderName { get; set; } = "Placeholder Name";
    public string SenderAddress { get; set; } = "Placeholder@address.com";
    public bool DevTestMode { get; set; } = false;
}

public class Graph
{
    public string BaseUrl { get; set; } = "https://graph.microsoft.com/v1.0";
    public string Scopes { get; set; } = "user.read";
}

public class Hosting
{
    public string Profile { get; set; } = "ssc";
    public string EnvironmentName { get; set; } = "dev";
    public int WorkspaceCountPerAzureSubscription { get; set; } = 100;
}

public class KeyVault
{
    public string UserName { get; set; } = null!;
}

/// <summary>
/// Represents the configuration settings for DatahubServiceBus.
/// </summary>
public class DatahubServiceBus
{
    public string ConnectionString { get; set; } = null!;
}

public class PublicFileSharing
{
    public string OpenDataApprovalPdfBaseUrl { get; set; } = null!;
    public string OpenDataApprovalPdfFormIdParam { get; set; } = null!;
    public string PublicFileSharingDomain { get; set; } = null!;
}
public class ReverseProxy
{
    public bool Enabled { get; set; } = true;
    public string UserHeader { get; set; } = "dh-user";
    public string WebAppPrefix { get; set; } = "app";
}

public class GithubConfig
{
    public bool Enabled { get; set; } = false;
    public string AppName { get; set; } = "datahub-integration";
    public string CallbackUrl { get; set; } = "https://localhost:5001/git/callback";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RepoPrefix { get; set; } = "fsdh-";
}

public class CkanConfiguration
{
    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = "https://registry-staging.open.canada.ca/";
    public bool TestMode { get; set; } = true;
    public bool PublishingEnabled { get; set; } = false;

    // ApiKey is now a per-project secret, but this property is still referenced by legacy code
    // it will be removed when cleaning up the old code
    public string ApiKey { get; set; } = string.Empty;

    private Uri BaseUri => new(BaseUrl);
    public string ApiUrl => new Uri(BaseUri, "api").ToString();
    public string DatasetUrl => new Uri(BaseUri, "dataset").ToString();

    public bool IsFeatureEnabled
    {
        get
        {
            var baseUrlConfigured = !string.IsNullOrEmpty(BaseUrl);

            return Enabled && baseUrlConfigured;
        }
    }
}