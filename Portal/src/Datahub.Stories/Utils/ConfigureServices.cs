using Datahub.Application;
using Datahub.Application.Configuration;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Offline;

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

        return services;
    }
        
}