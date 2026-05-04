using Azure.Core;
using Azure.Identity;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace Datahub.Infrastructure.Services.Security;

public class UserTokenCredentialService : IUserTokenCredentialService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<UserTokenCredentialService> _logger;  
    public UserTokenCredentialService(
        DatahubPortalConfiguration portalConfiguration,
        ITokenAcquisition tokenAcquisition,
        ILogger<UserTokenCredentialService> logger)
    {
        _portalConfiguration = portalConfiguration;
        this._tokenAcquisition = tokenAcquisition;
        _logger = logger;
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
