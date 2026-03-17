using Datahub.Infrastructure.Services.Azure;

namespace Datahub.Portal.Services;

public class AzureServicePrincipalConfig : IAzureServicePrincipalConfig
{
    private readonly IConfiguration _config;

    public AzureServicePrincipalConfig(IConfiguration config)
    {
        _config = config;
    }

    public string SubscriptionId => _config["AzureAD:SubscriptionId"]?? throw new InvalidOperationException("SubscriptionId not found");
    public string TenantId => _config["AzureAD:TenantId"]?? throw new InvalidOperationException("TenantId not found");
    public string ClientId => _config["AzureAD:ClientId"]?? throw new InvalidOperationException("ClientId not found");
    public string ClientSecret => _config["AzureAD:ClientSecret"]?? throw new InvalidOperationException("ClientSecret not found");
}
