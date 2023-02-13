using Newtonsoft.Json;

namespace Datahub.Application.Configuration;

public class DatahubPortalConfiguration
{
    public CultureSettings CultureSettings { get; set; } = new();
    public string LandingBgFolder { get; set; } = "ssc";
    public int LandingBgImgCount { get; set; } = 2;
    public string Title { get; set; } = "Federal Science DataHub default title";
    public string DataHubModules { get; set; } = null!;
    public DataProjects DataProjects { get; set; } = new();
    public Achievements Achievements { get; set; } = new();

    public ConnectionStrings ConnectionStrings { get; set; } = new();
    public AzureAd AzureAd { get; set; } = new();
    public Graph Graph { get; set; } = new();
    public APITargets APITargets { get; set; } = new();
    public ApplicationInsights ApplicationInsights { get; set; } = new();
    public KeyVault KeyVault { get; set; } = new();
    public Hosting Hosting { get; set; } = new();
    public EmailNotification EmailNotification { get; set; } = new();
    public PublicFileSharing PublicFileSharing { get; set; } = new();

    [JsonProperty("Azure:SignalR:StickyServerMode")]
    public string AzureSignalRStickyServerMode { get; set; } = "Required";

    public string AllowedHosts { get; set; } = null!;
    public DatahubStorageQueue DatahubStorageQueue { get; set; } = new();
    public string DatahubGraphInviteFunctionUrl { get; set; } = null!;
}

public class Achievements
{
    public bool Enabled { get; set; } = true;
    public bool LocalAchievementsOnly { get; set; } = false;
}

public class CultureSettings
{
    public string Default { get; set; } = "en-CA";
    public string SupportedCultures { get; set; } = "fr-CA|en-CA";
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
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string CallbackPath { get; set; } = "/signin-oidc";

    [JsonProperty("SignedOutCallbackPath ")]
    public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";

    public string AppIDURL { get; set; } = null!;
}

public class ConnectionStrings
{
    [JsonProperty("datahub-mssql-project")]
    public string? DatahubMsSqlProject { get; set; } 

    [JsonProperty("datahub-mssql-pip")] 
    public string? DatahubMsSqlPip { get; set; }

    [JsonProperty("datahub-mssql-etldb")]
    public string? DatahubMsSqlEtldb { get; set; }

    [JsonProperty("datahub-mssql-finance")]
    public string? DatahubMsSqlFinance { get; set; }

    [JsonProperty("datahub-mssql-webanalytics")]
    public string? DatahubMsSqlWebAnalytics { get; set; }

    [JsonProperty("datahub-mssql-metadata")]
    public string? DatahubMsSqlMetadata { get; set; }

    [JsonProperty("DATAHUB-MSSQL-LANGUAGETRAINING")]
    public string? DatahubMsSqlLanguageTraining { get; set; }

    [JsonProperty("datahub-cosmosdb")]
    public string? DatahubCosmosDb { get; set; }
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
}

public class KeyVault
{
    public string UserName { get; set; } = null!;
}

public class DatahubStorageQueue
{
    public string ConnectionString { get; set; } = null!;
    public QueueNames QueueNames { get; set; } = new();
}

public class QueueNames
{
    public string ResourceRunRequest { get; set; } = "resource-run-request";
    public string EmailNotification { get; set; } = "email-notifications";
    public string StorageCapacity { get; set; } = "storage-capacity";
    public string TerraformOutput { get; set; } = "terraform-output";
}

public class PublicFileSharing
{
    public string OpenDataApprovalPdfBaseUrl { get; set; } = null!;
    public string OpenDataApprovalPdfFormIdParam { get; set; } = null!;
    public string PublicFileSharingDomain { get; set; } = null!;
}