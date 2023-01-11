using Datahub.Core.RoleManagement;
using Datahub.Core.Services.UserManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

namespace Datahub.Portal.Services.Auth;

public static class ConfigureAuthServices
{

    public static void AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            // .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => configuration.Bind("Jwt", opts))
            .AddMicrosoftIdentityWebApp(configuration)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(configuration.GetSection("Graph"))
            .AddInMemoryTokenCaches();
        
        services.AddMicrosoftIdentityConsentHandler();

        // services.AddSession(options =>
        // {
        //     options.Cookie.IsEssential = true;
        //     options.Cookie.HttpOnly = true;
        // });

        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.Configure<SessionsConfig>(configuration.GetSection("Sessions"));
    }
}