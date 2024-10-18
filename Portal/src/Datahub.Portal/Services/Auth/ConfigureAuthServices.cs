using Datahub.Application;
using Datahub.Application.RoleManagement;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Datahub.Portal.Services.Auth;

public static class ConfigureAuthServices
{

    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = "https://login.microsoftonline.com/" + configuration["AzureAd:TenantId"] + "/v2.0";
            options.Audience = "api://" + configuration["AzureAd:ClientId"];
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
            };
            // IdentityModelEventSource.ShowPII = true; - for troubleshooting
        })
            //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => configuration.Bind("Jwt", opts))
            .AddMicrosoftIdentityWebApp(configuration)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(configuration.GetSection("Graph"))
            .AddInMemoryTokenCaches();

        services.AddMicrosoftIdentityConsentHandler();
        
        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.Configure<SessionsConfig>(configuration.GetSection("Sessions"));
    }
}