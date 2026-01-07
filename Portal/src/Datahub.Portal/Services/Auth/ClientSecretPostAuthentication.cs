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
            var claims = new List<Claim>
            {
                new("iss", clientId),
                new("aud", authority),
                new("client_id", clientId),
                new("response_type", responseType),
                new("redirect_uri", redirectUri),
                new("scope", scope),
                new("nonce", nonce),
                new("ui_locales", uiLocales),
                new("acr_values", AcrValuesClaimValue)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clientSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                clientId,
                authority,
                claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            var handler = new JwtSecurityTokenHandler();
            var requestJwt = handler.WriteToken(token);

            context.ProtocolMessage.SetParameter("request", requestJwt);

            // Remove duplicate parameters that are already in the signed request object
            // to avoid sending them twice (once in query/body, once in request object)
            context.ProtocolMessage.Scope = null;
            context.ProtocolMessage.ResponseType = null;
            // We keep client_id (and redirect_uri) typically to identify the request at the entry point,
            // but technically they are redundant. If the user wants them gone:
            context.ProtocolMessage.RedirectUri = null;
            // context.ProtocolMessage.ClientId = null; // Some IdPs require ClientId in query to identify the client first
            context.ProtocolMessage.Nonce = null;
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
