using Azure.Core;
using Azure.Identity;
using Datahub.Application.Services.Security;
using Datahub.Shared.Configuration;
using Microsoft.Extensions.Logging;


namespace Datahub.Infrastructure.Services.Security;

public class InfraSystemTokenCredentialService : ISystemTokenCredentialService
{

    private readonly ILogger<InfraSystemTokenCredentialService> _logger;
    private readonly AzureDevOpsConfiguration _config;

    public InfraSystemTokenCredentialService(AzureDevOpsConfiguration config, ILogger<InfraSystemTokenCredentialService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public TokenCredential GetPortalTokenCredential()
    {

        var tenantId = _config.TenantId;
        var clientId = _config.ClientId;
        var clientSecret = _config.ClientSecret;

        _logger.LogInformation("Using client secret token credential");
        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

    public TokenCredential GetInfraTokenCredential()
    {
        //if (_portalConfiguration.PortalRunAsManagedIdentity.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
        //{
        //    _logger.LogInformation("Using managed identity token credential");
        //    return new ManagedIdentityCredential();
        //}

        var tenantId = _config.TenantId;
        var clientId = _config.ClientId;
        var clientSecret = _config.ClientSecret;

        _logger.LogInformation("Using client secret token credential");
        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }


}
