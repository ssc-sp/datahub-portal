using Datahub.Infrastructure.Services.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Datahub.Functions;

public class AzureConfig : IAzureServicePrincipalConfig
{
    private readonly IConfiguration _config;
    private readonly EmailNotification _emailConfig;
    private readonly AdoConfig _adoConfig;

    public AzureConfig(IConfiguration config)
    {
        _config = config;
        _emailConfig = new EmailNotification();
        _adoConfig = new AdoConfig();
        _config.Bind("EmailNotification", _emailConfig);
        _config.Bind("AdoConfig", _adoConfig);
    }

    public EmailNotification Email => _emailConfig;
    
    public AdoConfig AdoConfig => _adoConfig;

    public string? NotificationPercents => _config["ProjectUsageNotificationPercents"];
    
    #region Inactivity
    
    public string? ProjectInactivityNotificationDays => _config["ProjectInactivityNotificationDays"] ?? "7,2";
    
    public string? ProjectInactivityDeletionDays => _config["ProjectInactivityDeletionDays"] ?? "180";
    
    public string? UserInactivityNotificationDays => _config["UserInactivityNotificationDays"] ?? "7,2";
    public string? UserInactivityLockedDays => _config["UserInactivityLockedDays"] ?? "30";
    public string? UserInactivityDeletionDays => _config["UserInactivityDeletionDays"] ?? "90";
    
    #endregion

    #region Azure SP

    public string TenantId => _config["TENANT_ID"] ?? "";
    public string ClientId => _config["FUNC_SP_CLIENT_ID"] ?? "";
    public string ClientSecret => _config["FUNC_SP_CLIENT_SECRET"] ?? "";
    public string SubscriptionId => _config["SUBSCRIPTION_ID"] ?? "";
    
    #endregion

    #region Storage Notifications

    public string StorageQueueConnection => _config["DatahubStorageConnectionString"] ?? "";
    public string MaxStorageCapacity => _config["MAX_STORAGE_CAPACITY"] ?? "180000000000"; //"2000000000000";
    public string UserRunRequestQueueName => _config["UserRunRequestQueueName"] ?? "user-run-request";
    
    #endregion

    public string PortalUrl => _config["PORTAL_URL"] ?? "";
    public string ServicePrincipalGroupID => _config["SP_GROUP_ID"] ?? "";
    public string KeyVaultName => _config["KEY_VAULT_NAME"] ?? "";
}

public class EmailNotification
{
    public bool DumpMessages { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SenderName { get; set; }
    public string? SenderAddress { get; set; }
    public string? NotificationsCCAddress { get; set; }
    public string? AdminEmail { get; set; } = "datasolutions-solutiondedonnees@ssc-spc.gc.ca";
    public bool IsValid => !string.IsNullOrEmpty(SmtpHost) && 
                           !string.IsNullOrEmpty(SmtpUsername) && 
                           !string.IsNullOrEmpty(SmtpPassword) && 
                           !string.IsNullOrEmpty(SenderAddress) &&
                           SmtpPort != 0;
}

public class AdoConfig
{
    public string OidSecretName { get; set; } = "ado-service-user-oid";
    public string PatSecretName { get; set; } = "ado-service-user-pat";
    public string OrgName { get; set; } = "DataSolutionsDonnees";
    public string ProjectName { get; set; } = "FSDH SSC";
    public string URL { get; set; } = "https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/${workItemTypeName}?api-version=6.0";
    public string PipelineURL { get; set; } = "https://dev.azure.com/{organization}/{project}/_apis/pipelines/{pipelineId}/runs?api-version=7.1-preview.1";
    public string AppServiceConfigPipelineId { get; set; } = "9";
}