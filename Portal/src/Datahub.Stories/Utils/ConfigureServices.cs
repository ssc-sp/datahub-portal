using Datahub.Application;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Offline;
using Microsoft.EntityFrameworkCore;

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
        
        // Add the EF Core DbContext
        services.AddDbContextFactory<DatahubProjectDBContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("DatahubProjectDBContext"));
        });

        services.AddScoped<PlaceholderService>();

        return services;
    }
        
}