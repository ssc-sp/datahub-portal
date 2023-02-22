namespace Datahub.Infrastructure.Services.Azure;

public interface IAzureServicePrincipalConfig
{
    string SubscriptionId { get; }
    string TenantId { get; }
    string ClientId { get; }
    string ClientSecret { get; }
}
