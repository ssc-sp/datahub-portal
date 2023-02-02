using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Datahub.Functions;

internal class AzureConfig
{
    private readonly IConfiguration _config;
    private readonly EmailConfig _emailConfig; 

    public AzureConfig(IConfiguration config)
    {
        _config = config;
        _emailConfig = new EmailConfig();
        _config.Bind("EmailService", _emailConfig);
    }

    public string LoginUrl => "https://login.microsoftonline.com/";
    public string ManagementUrl => "https://management.azure.com/";
    public EmailConfig Email => _emailConfig;

    #region Azure SP

    public string TenantId => _config["TENANT_ID"] ?? "";
    public string ClientId => _config["FUNC_SP_CLIENT_ID"] ?? "";
    public string ClientSecret => _config["FUNC_SP_CLIENT_SECRET"] ?? "";
    public string SubscriptionId => _config["SUBSCRIPTION_ID"] ?? "";
    
    #endregion

    #region Storage Notifications

    public string StorageQueueConnection => _config["datahub-storage-connection"] ?? "";
    public string MaxStorageCapacity => _config["MAX_STORAGE_CAPACITY"] ?? "180000000000"; //"2000000000000";
    
    #endregion
}

internal class EmailConfig
{
    public string? SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? FromAddress { get; set; }
    public bool IsValid => !string.IsNullOrEmpty(SmtpServer) && 
                           !string.IsNullOrEmpty(SmtpUsername) && 
                           !string.IsNullOrEmpty(SmtpPassword) && 
                           !string.IsNullOrEmpty(FromAddress) &&
                           SmtpPort != 0;
}