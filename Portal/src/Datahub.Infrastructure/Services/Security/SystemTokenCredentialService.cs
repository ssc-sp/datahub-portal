using Azure.Core;
using Azure.Identity;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Datahub.Infrastructure.Services.Security;

public class SystemTokenCredentialService : ISystemTokenCredentialService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;    
    private readonly ILogger<SystemTokenCredentialService> _logger;

    public SystemTokenCredentialService(
        DatahubPortalConfiguration portalConfiguration,
        ILogger<SystemTokenCredentialService> logger)
    {
        _portalConfiguration = portalConfiguration;
        _logger = logger;
    }

    public TokenCredential GetPortalTokenCredential()
    {
        if (_portalConfiguration.PortalRunAsManagedIdentity.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogInformation("Using managed identity token credential");
            return new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);
        }

        var tenantId = _portalConfiguration.AzureAd.TenantId;
        var clientId = _portalConfiguration.AzureAd.ClientId;
        var clientSecret = _portalConfiguration.AzureAd.ClientSecret;

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

        var tenantId = _portalConfiguration.AzureAd.TenantId;
        var clientId = _portalConfiguration.AzureAd.InfraClientId;
        var clientSecret = _portalConfiguration.AzureAd.InfraClientSecret;


        _logger.LogInformation("Using client secret token credential");
        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }


}
