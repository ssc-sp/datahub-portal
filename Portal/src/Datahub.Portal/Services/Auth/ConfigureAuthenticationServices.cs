using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Datahub.Application;
using Datahub.Core.Configuration;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.FeatureManagement;
using Datahub.Application.Authentication;

namespace Datahub.Portal.Services.Auth;

public static class ConfigureAuthenticationServices
{
    public const string GccfOidcScheme = "gccf-oidc";
    public const string GccfCookieScheme = "gccf-cookie"; // Define a separate cookie scheme for GCCF
    public const string GccfCookieName = "GccfAuth"; // Centralized cookie name constant
    public const string DefaultCookieName = ".AspNetCore.Cookies"; // Default cookie name used by Microsoft Identity

    public const string GccfSigninURL = "/gccf/signin-oidc";
    private const string CompositeCookieScheme = "composite-cookie";

    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var devAuthEmail = configuration["GccfOidc:DevAuth:UserEmail"];

        // Base authentication: default cookie is provided by AddMicrosoftIdentityWebApp + Azure AD OIDC
        services.AddAuthentication(options =>
        {
            if (!string.IsNullOrWhiteSpace(devAuthEmail))
            {
                options.DefaultScheme = DevAuthHandler.Scheme;
                options.DefaultAuthenticateScheme = DevAuthHandler.Scheme;
                options.DefaultSignInScheme = DevAuthHandler.Scheme;
                options.DefaultChallengeScheme = DevAuthHandler.Scheme;
            }
            else
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            }
        })
        .AddMicrosoftIdentityWebApp(configuration, "AzureAd", OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme)
        .EnableTokenAcquisitionToCallDownstreamApi()
        .AddMicrosoftGraph(configuration.GetSection("Graph"))
        .AddInMemoryTokenCaches();

        // Register dev scheme at the root AuthenticationBuilder level
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.Scheme, _ => { });

        // add the JWT bearer authentication for APIs
        services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = "https://sts.windows.net/" + configuration["AzureAd:TenantId"];
                options.Audience = "https://management.core.windows.net/";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
            });

        // Conditionally add GCCF OIDC provider and composite cookie selector based on feature flag
        var gccfEnabled = false;

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var featureManager = scope.ServiceProvider.GetService<IFeatureManagerSnapshot>();
        if (featureManager is not null)
        {
            gccfEnabled = featureManager.IsEnabledAsync(Features.GCCF_Feature).GetAwaiter().GetResult();
        }

        if (gccfEnabled)
        {
            // Reconfigure defaults to use composite selector only when dev auth is not forcing defaults
            services.AddAuthentication(options =>
            {
                if (!string.IsNullOrWhiteSpace(devAuthEmail))
                {
                    options.DefaultScheme = DevAuthHandler.Scheme;
                    options.DefaultAuthenticateScheme = DevAuthHandler.Scheme;
                    options.DefaultSignInScheme = DevAuthHandler.Scheme;
                    options.DefaultChallengeScheme = DevAuthHandler.Scheme;
                }
                else
                {
                    options.DefaultScheme = CompositeCookieScheme;
                    options.DefaultAuthenticateScheme = CompositeCookieScheme;
                    options.DefaultSignInScheme = CompositeCookieScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                }
            })
            .AddPolicyScheme(CompositeCookieScheme, CompositeCookieScheme, policyOptions =>
            {
                policyOptions.ForwardDefaultSelector = context =>
                {
                    // Favor Microsoft Identity default cookie first, then GCCF cookie
                    var hasDefaultCookie = context.Request.Cookies.ContainsKey(DefaultCookieName);
                    if (hasDefaultCookie)
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    var hasGccfCookie = context.Request.Cookies.ContainsKey(GccfCookieName);
                    if (hasGccfCookie)
                        return GccfCookieScheme;
                    // Fallback to default cookie scheme
                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            });

            ConfigurationHelper.DumpRedactedToConsole("GCCF Enabled. Client ID", configuration["GccfOidc:ClientId"], configuration["GccfOidc:ClientSecret"]);
            // Add a cookie scheme specifically for GCCF to avoid conflicts with the main "Cookies" scheme
            services.AddAuthentication()
                .AddCookie(GccfCookieScheme, options =>
                {
                    options.Cookie.Name = GccfCookieName;
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                })
                .AddOpenIdConnect(GccfOidcScheme, options =>
                {
                    // Use the dedicated cookie scheme for persistence
                    options.SignInScheme = GccfCookieScheme;
                    var authority = configuration["GccfOidc:Authority"]?.TrimEnd('/') ?? throw new ArgumentNullException("GCCF OIDC Authority");
                    options.Authority = authority;

                    // Safely combine authority and the metadata path
                    var metadataPath = "auth/gceab/oidc/private/.well-known/openid-configuration";
                    if (!string.IsNullOrEmpty(authority))
                    {
                        options.MetadataAddress = $"{authority}/{metadataPath}";
                    }
                    else
                    {
                        options.MetadataAddress = metadataPath; // fallback, will likely fail but keeps behavior explicit
                    }
                    options.ClientId = configuration["GccfOidc:ClientId"] ?? throw new ArgumentNullException("GCCF ClientID");
                    options.ClientSecret = configuration["GccfOidc:ClientSecret"] ?? throw new ArgumentNullException("GCCF ClientSecret");
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.CallbackPath = GccfSigninURL;
                    options.SaveTokens = true;
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.UsePkce = true;
                    options.Events.OnAuthorizationCodeReceived = context =>
                    {
                        context.Backchannel.SetBasicAuthenticationOAuth(context.TokenEndpointRequest.ClientId, context.TokenEndpointRequest.ClientSecret);
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        // Check if locale is set in AuthenticationProperties
                        if (context.Properties.Parameters.TryGetValue("ui_locales", out var localeObj) && localeObj is string locale)
                        {
                            context.ProtocolMessage.SetParameter("ui_locales", locale);
                        }

                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        // Check if locale is set in AuthenticationProperties
                        if (context.Properties.Parameters.TryGetValue("ui_locales", out var localeObj) && localeObj is string locale)
                        {
                            context.ProtocolMessage.SetParameter("ui_locales", locale);
                        }

                        return Task.CompletedTask;
                    };
                });
        }

        services.AddMicrosoftIdentityConsentHandler();

        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.Configure<SessionsConfig>(configuration.GetSection("Sessions"));
    }
}
