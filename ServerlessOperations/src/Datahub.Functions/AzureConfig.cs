using Datahub.Infrastructure.Services.Azure;
using Datahub.Shared.Configuration;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions;

public class AzureConfig : IAzureServicePrincipalConfig
{
    private readonly IConfiguration _config;
    private readonly EmailNotification _emailConfig;
    private readonly AzureDevOpsConfiguration _azureDevOpsConfiguration;

    public AzureConfig(IConfiguration config)
    {
        _config = config;
        _emailConfig = new EmailNotification();
        _azureDevOpsConfiguration = new AzureDevOpsConfiguration();

        _config.Bind("EmailNotification", _emailConfig);
        _config.Bind("AzureDevOpsConfiguration", _azureDevOpsConfiguration);
    }

    public EmailNotification Email => _emailConfig;
    
    public AzureDevOpsConfiguration AzureDevOpsConfiguration => _azureDevOpsConfiguration;

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
    
    public string MediaStorageConnectionString => _config["Media:StorageConnectionString"] ?? "";

    /// <summary>
    /// Timespan between alerts for infrastructure health checks. This uses TimeSpan format. 
    /// e.g. "1" = 1 day; "1:23" = 1 hour 23 minutes; "1.6:00" => 1 day 6 hours.
    /// For more information: https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-timespan-parse
    /// </summary>
    public string InfrastructureAlertDebounceTimeSpan => _config["InfrastructureAlertDebounceTimeSpan"] ?? "1";

    public string? BugReportTeamsWebhookUrl => _config["BugReportTeamsWebhookUrl"];
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