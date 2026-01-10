using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Datahub.Portal.Services.Auth;

public static class ClientSecretPostAuthenticationExtensions
{
    public static OpenIdConnectEvents UseClientSecretPostAuthentication(this OpenIdConnectOptions options)
    {
        options.Events = new ClientSecretPostAuthentication();
        return options.Events;
    }
}

public class ClientSecretPostAuthentication : OpenIdConnectEvents
{
    private const string UiLocalesItemsKey = "ui_locales";
    private const string AcrValuesClaimValue = "urn:gc-ca:cyber-auth:assurance:loa2";
    private const string DefaultUiLocales = "en-CA";

    public ClientSecretPostAuthentication()
    {
        OnRedirectToIdentityProvider = HandleRedirectToIdentityProvider;
        OnAuthorizationCodeReceived = HandleAuthorizationCodeReceived;
    }

    private Task HandleRedirectToIdentityProvider(RedirectContext context)
    {
        // Gather standard OIDC parameters
        var clientId = context.Options.ClientId;
        var authority = context.Options.Authority ?? throw new ArgumentNullException(nameof(context.Options.Authority));
        var redirectUri = context.ProtocolMessage.RedirectUri;
        var responseType = context.ProtocolMessage.ResponseType;
        var scope = context.ProtocolMessage.Scope;
        var nonce = context.ProtocolMessage.Nonce;
        
        var state = context.ProtocolMessage.State;
        if (string.IsNullOrEmpty(state))
        {
            state = context.Options.StateDataFormat.Protect(context.Properties);
            context.ProtocolMessage.State = state;
        }

        var clientSecret = context.Options.ClientSecret;

        // Get ui_locales from properties passed by the controller (or fallback)
        string? uiLocales = null;
        if (context.Properties.Items.TryGetValue(UiLocalesItemsKey, out var specifiedLocale) && !string.IsNullOrWhiteSpace(specifiedLocale))
        {
            uiLocales = specifiedLocale;
        }
        else
        {
            // Fallback or throw if you prefer strictness, keeping inline logic logic where it threw
            throw new InvalidOperationException("ui_locales not specified in authentication properties.");
        }

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            var now = DateTime.UtcNow;

            // Build request object claims per OIDC Request Object spec
            var claims = new List<Claim>
            {
                new("iss", clientId), // issuer must be the client_id
                new("aud", authority), // audience should be the OP issuer/authority
                new("client_id", clientId),
                new("response_type", responseType),
                new("redirect_uri", redirectUri),
                new("scope", scope),
                new("nonce", nonce),
                new("state", state),
                new("ui_locales", uiLocales),
                new("acr_values", AcrValuesClaimValue),
                //new("iat", ((DateTimeOffset)now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
                //new("nbf", ((DateTimeOffset)now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
                new("exp", ((DateTimeOffset)now.AddMinutes(5)).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
                //new("jti", Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clientSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create JWT with custom claims and explicit lifetime
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = clientId,
                Audience = authority,
                Claims = claims.ToDictionary(c => c.Type, c => (object)c.Value),
                NotBefore = now,
                Expires = now.AddMinutes(5),
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateJwtSecurityToken(tokenDescriptor);
            var requestJwt = handler.WriteToken(securityToken);

            context.ProtocolMessage.SetParameter("request", requestJwt);
        }

        return Task.CompletedTask;
    }

    private Task HandleAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
    {
        // The OIDC handler posts x-www-form-urlencoded to the token endpoint automatically.
        // Setting these ensures client_id and client_secret are in the POST body.
        context.TokenEndpointRequest.ClientId = context.Options.ClientId;
        context.TokenEndpointRequest.ClientSecret = context.Options.ClientSecret;
        return Task.CompletedTask;
    }
}
