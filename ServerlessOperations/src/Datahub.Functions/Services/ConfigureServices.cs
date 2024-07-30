using Datahub.Application.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Functions.Services;

public static class ConfigureServices
{
    private const string TENANT_ID_KEY = "TENANT_ID";
    private const string PORTAL_CLIENT_ID_KEY = "FUNC_SP_CLIENT_ID";
    private const string PORTAL_CLIENT_SECRET_KEY = "FUNC_SP_CLIENT_SECRET";
    private const string DEVOPS_CLIENT_ID_KEY = "AzureDevOpsConfiguration:ClientId";
    private const string DEVOPS_CLIENT_SECRET_KEY = "AzureDevOpsConfiguration:ClientSecret";
    private const string DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY = "DatahubServiceBus:ConnectionString";


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

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.TenantId))
        {
            datahubConfiguration.AzureAd.TenantId = configuration[TENANT_ID_KEY]
                                                    ?? throw new ArgumentNullException(TENANT_ID_KEY);
        }

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.ClientId))
        {
            datahubConfiguration.AzureAd.ClientId = configuration[PORTAL_CLIENT_ID_KEY]
                                                    ?? throw new ArgumentNullException(PORTAL_CLIENT_ID_KEY);
        }

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.ClientSecret))
        {
            datahubConfiguration.AzureAd.ClientSecret = configuration[PORTAL_CLIENT_SECRET_KEY]
                                                        ?? throw new ArgumentNullException(PORTAL_CLIENT_SECRET_KEY);
        }

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.InfraClientId))
        {
            datahubConfiguration.AzureAd.InfraClientId = configuration[DEVOPS_CLIENT_ID_KEY]
                ?? throw new ArgumentNullException(DEVOPS_CLIENT_ID_KEY);
        }

        if (string.IsNullOrEmpty(datahubConfiguration.AzureAd.InfraClientSecret))
        {
            datahubConfiguration.AzureAd.InfraClientSecret = configuration[DEVOPS_CLIENT_SECRET_KEY]
                ?? throw new ArgumentNullException(DEVOPS_CLIENT_SECRET_KEY);
        }

        if (string.IsNullOrEmpty(datahubConfiguration.DatahubServiceBus.ConnectionString))
        {
            datahubConfiguration.DatahubServiceBus.ConnectionString =
                configuration[DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY]
                ?? throw new ArgumentNullException(DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY);
        }

        services.AddSingleton(datahubConfiguration);
        services.AddMassTransitForAzureFunctions(x =>
        {
            x.AddConsumersFromNamespaceContaining<EmailNotificationHandler>();
        }, DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY);

        return services;
    }
}