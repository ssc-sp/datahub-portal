using Datahub.Infrastructure.Services.Azure;

namespace Datahub.Portal.Services;

public class AzureServicePrincipalConfig : IAzureServicePrincipalConfig
{
    private readonly IConfiguration _config;

    public AzureServicePrincipalConfig(IConfiguration config)
    {
        _config = config;
    }

    public string SubscriptionId => _config["AzureAD:SubscriptionId"];
    public string TenantId => _config["AzureAD:TenantId"];
    public string ClientId => _config["AzureAD:ClientId"];
    public string ClientSecret => _config["AzureAD:ClientSecret"];
}
