using Datahub.Application.Configuration;
using Datahub.Core.Configuration;
using Datahub.Shared.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddPortalConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var datahubConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubConfiguration);
        services.AddSingleton(datahubConfiguration);
        services.AddSingleton<IAzureConfiguration>(datahubConfiguration);
        return services;
    }
}
