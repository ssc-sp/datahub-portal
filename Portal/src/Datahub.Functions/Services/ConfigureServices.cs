using Datahub.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Functions.Services;

public static class ConfigureServices
{
    /// <summary>
    /// Adds Datahub configuration from Azure function format to the service collections.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the configuration to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> used for binding the configuration.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required configuration value is null or empty.</exception>
    public static IServiceCollection AddDatahubConfigurationFromFunctionFormat(this IServiceCollection services,
        IConfiguration configuration)
    {
        var datahubConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubConfiguration);

        if (string.IsNullOrEmpty(datahubConfiguration.DatahubStorageQueue.ConnectionString))
        {
            datahubConfiguration.DatahubStorageQueue.ConnectionString = configuration["DatahubStorageConnectionString"]
                                                                        ?? throw new ArgumentNullException(
                                                                            "DatahubStorageConnectionString");
        }

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.ClientId))
        {
            datahubConfiguration.AzureAd.ClientId = configuration["FUNC_SP_CLIENT_ID"]
                                                    ?? throw new ArgumentNullException("FUNC_SP_CLIENT_ID");
        }

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.ClientSecret))
        {
            datahubConfiguration.AzureAd.ClientSecret = configuration["FUNC_SP_CLIENT_SECRET"]
                                                        ?? throw new ArgumentNullException("FUNC_SP_CLIENT_SECRET");
        }

        services.AddSingleton(datahubConfiguration);

        return services;
    }
}