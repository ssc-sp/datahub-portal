using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Datahub.Application;
using Datahub.Application.RoleManagement;
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

namespace Datahub.Portal.Services.Auth;

public static class ConfigureAuthServices
{
    public const string GccfOidcScheme = "gccf-oidc";
    public const string GccfCookieScheme = "gccf-cookie"; // Define a separate cookie scheme for GCCF

    public const string GccfSigninURL = "/gccf/signin-oidc";

    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // configure the primary Azure AD authentication
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(configuration, "AzureAd", OpenIdConnectDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(configuration.GetSection("Graph"))
            .AddInMemoryTokenCaches();

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

        // Add a cookie scheme specifically for GCCF to avoid conflicts with the main "Cookies" scheme
        services.AddAuthentication()
            .AddCookie(GccfCookieScheme, options =>
            {
                options.Cookie.Name = "GccfAuth";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });

        // add the second OIDC provider
        services.AddAuthentication()
            .AddOpenIdConnect(GccfOidcScheme, options =>
            {
                // Use the dedicated cookie scheme for persistence
                options.SignInScheme = GccfCookieScheme;

                // these URLs are temporary here, they are for testing purposes in dev only
                options.Authority = "https://te-gc.auth.canada.ca";
                options.MetadataAddress = "https://te-gc.auth.canada.ca/auth/gceab/oidc/private/.well-known/openid-configuration";
                options.ClientId = "fsdh-gccf-oidc";
                options.ClientSecret = configuration["GccfOidc:ClientSecret"]; // From configuration
                options.ResponseType = OpenIdConnectResponseType.Code;

                options.CallbackPath = GccfSigninURL;
                options.SaveTokens = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.UseClientSecretPostAuthentication();
            });

        services.AddMicrosoftIdentityConsentHandler();

        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.Configure<SessionsConfig>(configuration.GetSection("Sessions"));
    }
}
