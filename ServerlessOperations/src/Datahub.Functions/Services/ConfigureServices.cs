using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Projects;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.WebApp;
using Datahub.Core.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.Projects;
using Datahub.Functions.Providers;
using Datahub.Infrastructure;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Infrastructure.Services.Notification;
using Datahub.Infrastructure.Services.Projects;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Infrastructure.Services.WebApp;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Functions.Services;

public static class ConfigureServices
{
    internal const string TENANT_ID_KEY = "TENANT_ID";
    internal const string PORTAL_CLIENT_ID_KEY = "FUNC_SP_CLIENT_ID";
    internal const string PORTAL_CLIENT_SECRET_KEY = "FUNC_SP_CLIENT_SECRET";
    internal const string DEVOPS_CLIENT_ID_KEY = "AzureDevOpsConfiguration:ClientId";
    internal const string DEVOPS_CLIENT_SECRET_KEY = "AzureDevOpsConfiguration:ClientSecret";
    internal const string DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY = "DatahubServiceBus:ConnectionString";

    public static IServiceCollection AddFunctionsHostServices(this IServiceCollection services)
    {
        services.AddScoped<IMSGraphService, MSGraphService>();
        services.AddScoped<IUserTokenCredentialService, ServerUserTokenProviderService>();
        services.AddScoped<IUserInformationService, FunctionUserInformationService>();
        services.AddScoped<AzureDevOpsClient>();
        services.AddScoped<AzAccessTokenManager>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddScoped<ILockedUserManagementService, LockedUserManagementService>();
        services.AddScoped<IKeyVaultUserService, ServerKeyVaultService>();
        services.AddScoped<IDateProvider, DateProvider>();
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<VirusScanNotificationHandler>();

        return services;
    }

    public static IServiceCollection AddDatahubConfigurationFromFunctionFormat(this IServiceCollection services,
        IConfiguration configuration)
    {
        var datahubConfiguration = new DatahubPortalConfiguration();
        configuration.Bind(datahubConfiguration);

        datahubConfiguration.AzureAd.TenantId = configuration[TENANT_ID_KEY]
                                                ?? throw new ArgumentNullException(TENANT_ID_KEY);

        datahubConfiguration.AzureAd.ClientId = configuration[PORTAL_CLIENT_ID_KEY]
                                                ?? throw new ArgumentNullException(PORTAL_CLIENT_ID_KEY);

        datahubConfiguration.AzureAd.ClientSecret = configuration[PORTAL_CLIENT_SECRET_KEY]
                                                    ?? throw new ArgumentNullException(PORTAL_CLIENT_SECRET_KEY);
        datahubConfiguration.AzureAd.InfraClientId = configuration[DEVOPS_CLIENT_ID_KEY]
            ?? throw new ArgumentNullException(DEVOPS_CLIENT_ID_KEY);

        datahubConfiguration.AzureAd.InfraClientSecret = configuration[DEVOPS_CLIENT_SECRET_KEY]
            ?? throw new ArgumentNullException(DEVOPS_CLIENT_SECRET_KEY);

        datahubConfiguration.DatahubServiceBus.ConnectionString =
            configuration[DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY]
            ?? throw new ArgumentNullException(DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY);

        services.AddMassTransitForAzureFunctions(x =>
        {
            x.AddConsumersFromNamespaceContaining<EmailNotificationHandler>();
        }, DATAHUB_SERVICE_BUS_CONNECTION_STRING_KEY);

        services
            .AddOptions<APITargets>()
            .Bind(configuration.GetSection(nameof(APITargets)))
            .Validate(o => !string.IsNullOrWhiteSpace(o.KeyVaultName), "KeyVaultName is required")
            .ValidateOnStart();

        ConfigurationHelper.DumpRedactedToConsole("Datahub configuration", datahubConfiguration);

        services.PostConfigure<APITargets>(apiTargets =>
        {
            ConfigurationHelper.DumpRedactedToConsole("API targets", apiTargets);
        });

        return services;
    }
}
