using Azure.Core;
using Azure.Identity;
using Datahub.Application.Services.Security;
using Datahub.Shared.Configuration;
using Microsoft.Extensions.Logging;


namespace Datahub.Infrastructure.Services.Security;

public class InfraTokenCredentialService : ISystemTokenCredentialService
{
    private readonly AzureDevOpsConfiguration _config;

    public InfraTokenCredentialService(AzureDevOpsConfiguration config)
    {
        _config = config;
    }

    public TokenCredential GetTokenCredential()
    {

        var tenantId = _config.TenantId;
        var clientId = _config.ClientId;
        var clientSecret = _config.ClientSecret;

        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }
}
