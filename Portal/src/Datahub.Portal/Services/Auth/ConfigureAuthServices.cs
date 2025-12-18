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

        // add the second OIDC provider
        services.AddAuthentication()
            .AddOpenIdConnect(GccfOidcScheme, options =>
            {
                options.Authority = "https://te-gc.auth.canada.ca";
              
                options.ClientId = "fsdh-gccf-oidc";
                options.ClientSecret = configuration["GccfOidc:ClientSecret"]; // From configuration
                options.ResponseType = "code";
                options.CallbackPath = GccfSigninURL;
                options.SaveTokens = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
            });

        services.AddMicrosoftIdentityConsentHandler();
        
        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.Configure<SessionsConfig>(configuration.GetSection("Sessions"));
    }
}