using Datahub.Application.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Datahub.Core.Configuration;
using Datahub.Core.Data; // added

namespace Datahub.Functions.Services;

public static class ConfigureServices
{
    private const string TENANT_ID_KEY = "TENANT_ID";
    private const string PORTAL_CLIENT_ID_KEY = "FUNC_SP_CLIENT_ID";
    private const string PORTAL_CLIENT_SECRET_KEY = "FUNC_SP_CLIENT_SECRET";
    private const string DEVOPS_CLIENT_ID_KEY = "AzureDevOpsConfiguration:ClientId";
    private const string DEVOPS_CLIENT_SECRET_KEY = "AzureDevOpsConfiguration:ClientSecret";
    private const string DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY = "DatahubServiceBus:ConnectionString";

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

        // APITargets via Options pattern (replaces manual singleton binding)
        services
            .AddOptions<APITarget>()
            .Bind(configuration.GetSection("APITargets"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.KeyVaultName), "KeyVaultName is required")
            .ValidateOnStart();

        // Diagnostic dump (redacted)
        ConfigurationHelper.DumpRedactedToConsole("Datahub configuration", datahubConfiguration);

        // Dump bound APITargets once (resolve from provider after options configured)
        services.PostConfigure<APITarget>(apiTargets =>
        {
            ConfigurationHelper.DumpRedactedToConsole("API targets", apiTargets);
        });

        return services;
    }
}