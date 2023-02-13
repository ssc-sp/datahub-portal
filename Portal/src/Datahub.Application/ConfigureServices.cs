using Datahub.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddDatahubApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var datahubConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubConfiguration);
        services.AddSingleton(datahubConfiguration);

        return services;
    }
}