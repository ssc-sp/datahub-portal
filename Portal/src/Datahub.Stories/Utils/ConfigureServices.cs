using Datahub.Application;
using Datahub.Core.Model.Datahub;
using Datahub.Core.RoleManagement;
using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Datahub.Infrastructure.Offline;
using Datahub.Metadata.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Datahub.Stories.Utils;

/// <summary>
/// This class is used to add the services required by the Blazing Story application.
/// </summary>
public static class ConfigureServices
{
    /// <summary>
    /// This method is used to add the services required by the Blazing Story application.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddDatahubBlazingStoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatahubApplicationServices(configuration);
        services.AddDatahubOfflineInfrastructureServices(configuration);

        services.AddTransient<IAuthenticationSchemeProvider, MockSchemeProvider>();
        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.AddSingleton<ServiceAuthManager>();
        services.AddScoped<UserLocationManagerService>();
        services.AddMudMarkdownServices();

        // Add the EF Core DbContexts
        services.AddDbContextFactory<DatahubProjectDBContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DatahubProjectDBContext"));
        });

        services.AddDbContextFactory<MetadataDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DatahubMetadataDBContext"));
        });

        services.AddScoped<PlaceholderService>();

        return services;
    }

}