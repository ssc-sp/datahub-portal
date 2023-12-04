using Datahub.Infrastructure.Services.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Datahub.Functions;

public class AzureConfig : IAzureServicePrincipalConfig
{
    private readonly IConfiguration _config;
    private readonly EmailNotification _emailConfig; 

    public AzureConfig(IConfiguration config)
    {
        _config = config;
        _emailConfig = new EmailNotification();
        _config.Bind("EmailNotification", _emailConfig);
    }

    public EmailNotification Email => _emailConfig;

    public string? NotificationPercents => _config["ProjectUsageNotificationPercents"];
    
    public string? ProjectInactivityNotificationDays => _config["ProjectInactivityNotificationDays"];
    
    public string? ProjectInactivityDeletionDays => _config["ProjectInactivityDeletionDays"];
    
    public string? UserInactivityNotificationDays => _config["UserInactivityNotificationDays"];
    public string? UserInactivityLockedDays => _config["UserInactivityLockedDays"];
    public string? UserInactivityDeletionDays => _config["UserInactivityDeletionDays"];

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
    public string? AdminEmail { get; set; }
    public bool IsValid => !string.IsNullOrEmpty(SmtpHost) && 
                           !string.IsNullOrEmpty(SmtpUsername) && 
                           !string.IsNullOrEmpty(SmtpPassword) && 
                           !string.IsNullOrEmpty(SenderAddress) &&
                           SmtpPort != 0;
}