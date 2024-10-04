using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Notebooks;
using Datahub.Application.Services.Notifications;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Announcements;
using Datahub.Infrastructure.Services.CatalogSearch;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.Notebooks;
using Datahub.Infrastructure.Services.Notifications;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Infrastructure.Services.ReverseProxy;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.Subscriptions;
using Datahub.Infrastructure.Services.UserManagement;
using MassTransit;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;

namespace Datahub.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddDatahubInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddMediatR(typeof(QueueMessageSender<>)); v11 mediatr code
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Datahub.Infrastructure.ConfigureServices).Assembly));
        services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
        services.AddScoped<IProjectUserManagementService, ProjectUserManagementService>();
        services.AddScoped<IProjectStorageConfigurationService, ProjectStorageConfigurationService>();
        services.AddScoped<CloudStorageManagerFactory>();
        services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<IProjectResourceWhitelistService, ProjectResourcingWhitelistService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IDatahubEmailService, DatahubEmailService>();
        services.AddScoped<IDatabricksApiService, DatabricksApiService>();
        services.AddScoped<IUsersStatusService, UsersStatusService>();
        services.AddSingleton<IDatahubCatalogSearch, DatahubCatalogSearch>();
        services.AddScoped<IDatahubAzureSubscriptionService, DatahubAzureSubscriptionService>();
        services.AddScoped<IUserInformationService, UserInformationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();


        services.AddAzureResourceManager(configuration);
        services.AddTransient<IWorkspaceCostManagementService, WorkspaceCostManagementService>();
        services.AddTransient<IWorkspaceResourceGroupsManagementService, WorkspaceResourceGroupsManagementService>();

        if (configuration.GetValue<bool>("ReverseProxy:Enabled"))
        {
            services.AddDatahubReverseProxyServices();
        }

        services.AddHostedService<PreloaderService>();
        services.AddMemoryCache();


        // in Development, using InMemory MassTransit transport, HealthCheckConsumer and file system (FileWatcherService)
        // to pass and process HealthCheck messages
        if (DevTools.IsDevelopment())
        {
            services.AddScoped<IHealthCheckConsumer, HealthCheckConsumer>();
            services.AddScoped<IHealthCheckResultConsumer, HealthCheckResultConsumer>();
            services.AddHostedService<LocalMessageReaderService>();
        }

        services.AddMassTransit(x =>
        {
            if (DevTools.IsDevelopment())
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                    cfg.ReceiveEndpoint("infrastructure-health-check",
                        endpoint => { endpoint.Consumer<HealthCheckConsumer>(); });
                    cfg.ReceiveEndpoint("infrastructure-health-check-results",
                        endpoint => { endpoint.Consumer<HealthCheckResultConsumer>(); });
                });
            }
            else
            {
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(configuration["DatahubServiceBus:ConnectionString"],
                        hc => hc.TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets);
                    cfg.PrefetchCount = 1;
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }

    public static IServiceCollection AddAzureResourceManager(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAzureClients(
            builder =>
            {
                builder.AddClient<ArmClient, ArmClientOptions>(options =>
                {
                    options.Diagnostics.IsLoggingEnabled = true;
                    options.Retry.Mode = RetryMode.Exponential;
                    options.Retry.MaxRetries = 5;
                    options.Retry.Delay = TimeSpan.FromSeconds(2);
                    var tenantId = configuration.GetValue<string>("AzureAd:TenantId") ??
                                   configuration.GetValue<string>("TENANT_ID");
                    var clientId = configuration.GetValue<string>("AzureAd:ClientId") ??
                                   configuration.GetValue<string>("FUNC_SP_CLIENT_ID");
                    var clientSecret = configuration.GetValue<string>("AzureAd:ClientSecret") ??
                                       configuration.GetValue<string>("FUNC_SP_CLIENT_SECRET");
                    var subscriptionId = configuration.GetValue<string>("AzureAd:SubscriptionId") ??
                                         configuration.GetValue<string>("SUBSCRIPTION_ID");
                    var creds = new ClientSecretCredential(tenantId, clientId, clientSecret);
                    var client = new ArmClient(creds, subscriptionId, options);
                    return client;
                });
            });
        return services;
    }
}