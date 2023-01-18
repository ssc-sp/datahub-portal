using Microsoft.Extensions.Configuration;

namespace Datahub.Functions;

internal class AzureConfig
{
    private readonly IConfiguration _config;
    
    public AzureConfig(IConfiguration config)
    {
        _config = config;
    }

    public string LoginUrl => "https://login.microsoftonline.com/";
    public string ManagementUrl => "https://management.azure.com/";

    #region Azure SP
    public string TenantId => _config["TENANT_ID"] ?? "";
    public string ClientId => _config["FUNC_SP_CLIENT_ID"] ?? "";
    public string ClientSecret => _config["FUNC_SP_CLIENT_SECRET"] ?? "";
    public string SubscriptionId => _config["SUBSCRIPTION_ID"] ?? "";
    #endregion

    #region Email Notifications
    public string EmailSmtpHost => _config["EMAIL_SMTP_HOST"] ?? "";
    public int EmailSmtpPort => int.Parse(_config["EMAIL_SMTP_PORT"] ?? "21");
    public string EmailSmtpFrom => _config["EMAIL_SMTP_FROM"] ?? "";
    public string EmailSmtpUser => _config["EMAIL_SMTP_USER"] ?? "";
    public string EmailSmtpPassword => _config["EMAIL_SMTP_PWD"] ?? "";
    #endregion

    #region Storage Notifications

    public string StorageQueueConnection => _config["datahub-storage-queue"] ?? "";
    public string MaxStorageCapacity => _config["MAX_STORAGE_CAPACITY"] ?? "180000000000"; //"2000000000000";
    #endregion
}
