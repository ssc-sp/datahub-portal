using Azure.Core;
using Datahub.Application.Services.Security;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Datahub.Infrastructure.Services.Security;

public class UserTokenCredentialService : IUserTokenCredentialService
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly ILogger<UserTokenCredentialService> _logger;
    private string? _currentUserVaultToken;

    public UserTokenCredentialService(
        ITokenAcquisition tokenAcquisition,
        ILogger<UserTokenCredentialService> logger)
    {
        _tokenAcquisition = tokenAcquisition;
        _logger = logger;
    }

    public Task<TokenCredential> GetTokenCredentialForUser(string? vaultToken = null)
    {
        var token = vaultToken ?? _currentUserVaultToken;
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Task.FromResult<TokenCredential>(new StaticAccessTokenCredential(token, _logger));
    }

    public async Task<string> GetUserToken(ClaimsPrincipal claimsPrincipal)
    {
        var scopes = new[] { "https://vault.azure.net/user_impersonation" };
        _currentUserVaultToken = await _tokenAcquisition.GetAccessTokenForUserAsync(
            scopes,
            authenticationScheme: OpenIdConnectDefaults.AuthenticationScheme,
            user: claimsPrincipal);

        _logger.LogInformation("Using on-behalf-of for user {UserId}", claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return _currentUserVaultToken;
    }

    private sealed class StaticAccessTokenCredential : TokenCredential
    {
        private readonly AccessToken _accessToken;

        public StaticAccessTokenCredential(string token, ILogger logger)
        {
            _accessToken = new AccessToken(token, GetTokenExpiry(token, logger));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => _accessToken;

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(_accessToken);

        private static DateTimeOffset GetTokenExpiry(string token, ILogger logger)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                {
                    return DateTimeOffset.UtcNow.AddMinutes(5);
                }

                var payload = parts[1]
                    .Replace('-', '+')
                    .Replace('_', '/');

                var padding = payload.Length % 4;
                if (padding != 0)
                {
                    payload = payload.PadRight(payload.Length + (4 - padding), '=');
                }

                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                using var document = JsonDocument.Parse(json);
                return document.RootElement.TryGetProperty("exp", out var expElement) && expElement.TryGetInt64(out var exp)
                    ? DateTimeOffset.FromUnixTimeSeconds(exp)
                    : DateTimeOffset.UtcNow.AddMinutes(5);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Unable to parse expiry from user Key Vault access token. Using a fallback expiry.");
                return DateTimeOffset.UtcNow.AddMinutes(5);
            }
        }
    }
}
