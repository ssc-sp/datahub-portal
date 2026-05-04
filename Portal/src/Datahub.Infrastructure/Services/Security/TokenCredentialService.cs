using Azure.Core;
using Azure.Identity;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Datahub.Infrastructure.Services.Security;

public class TokenCredentialService : ITokenCredentialService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<TokenCredentialService> _logger;

    public TokenCredentialService(
        DatahubPortalConfiguration portalConfiguration,
        ITokenAcquisition tokenAcquisition,
        ILogger<TokenCredentialService> logger)
    {
        _portalConfiguration = portalConfiguration;
        this._tokenAcquisition = tokenAcquisition;
        _logger = logger;
    }

    public TokenCredential GetTokenCredential()
    {
        if (_portalConfiguration.PortalRunAsManagedIdentity.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogInformation("Using managed identity token credential");
            return new ManagedIdentityCredential();
        }

        var tenantId = _portalConfiguration.AzureAd.TenantId;
        var clientId = _portalConfiguration.AzureAd.ClientId;
        var clientSecret = _portalConfiguration.AzureAd.ClientSecret;

        _logger.LogInformation("Using client secret token credential");
        return new ClientSecretCredential(tenantId, clientId, clientSecret);
    }

    public async Task<TokenCredential> GetTokenCredentialForUser(string vaultToken)
    {
        //var obo = new OnBehalfOfCredential()
        //previous code
        //var user = await _userInfoService.GetAuthenticatedUser();
        var tenantId = _portalConfiguration.AzureAd.TenantId;
        var clientId = _portalConfiguration.AzureAd.ClientId;
        var clientSecret = _portalConfiguration.AzureAd.ClientSecret;
        return new OnBehalfOfCredential(tenantId, clientId, clientSecret, vaultToken);
    }

    public async Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal)
    {
        var scopes = new string[] { "https://vault.azure.net/user_impersonation" };
        var vaultToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes, authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme, user: claimsPrincipal);
        _logger.LogInformation("Using on-behalf-of for user {UserId}", claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return vaultToken;
    }
}
