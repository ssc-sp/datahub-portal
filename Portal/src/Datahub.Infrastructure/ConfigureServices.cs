using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.Notebooks;
using Datahub.Application.Services.Notifications;
using Datahub.Application.Services.Projects;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.Toolbox;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.WebApp;
using Datahub.Application.Services.Notification;
using Datahub.Core;
using Datahub.Core.Configuration;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Core.Services.Projects;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Announcements;
using Datahub.Infrastructure.Services.CatalogSearch;
using Datahub.Infrastructure.Services.Cost;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Infrastructure.Services.Notebooks;
using Datahub.Infrastructure.Services.Notifications;
using Datahub.Infrastructure.Services.Projects;
using Datahub.Infrastructure.Services.ResourceGroups;
using Datahub.Infrastructure.Services.ReverseProxy;
using Datahub.Infrastructure.Services.Security;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.Subscriptions;
using Datahub.Infrastructure.Services.Toolbox;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Infrastructure.Services.VirusScan;
using Datahub.Infrastructure.Services.WebApp;
using Datahub.Infrastructure.Services.Notification;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddPortalInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddMediatR(typeof(QueueMessageSender<>)); v11 mediatr code
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Datahub.Infrastructure.ConfigureServices).Assembly));
        services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
        services.AddScoped<ILockedUserManagementService, LockedUserManagementService>();
        services.AddScoped<IProjectStorageConfigurationService, ProjectStorageConfigurationService>();
        services.AddSingleton<IFileTokenService, FileTokenService>();
        services.AddScoped<CloudStorageManagerFactory>();
        services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<HealthCheckHelper>();
        services.AddScoped<ISubnetPoolService, SubnetPoolService>();
        services.AddScoped<IProjectResourceWhitelistService, ProjectResourcingWhitelistService>();
        services.AddSingleton<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IDatahubEmailService, DatahubEmailService>();
        services.AddScoped<IUsersStatusService, UsersStatusService>();
        services.AddScoped<IDatahubAzureSubscriptionService, DatahubAzureSubscriptionService>();
        services.AddScoped<INetworkingManagementService, NetworkingManagementService>();
        services.AddScoped<IExternalUserInvitationService, ExternalUserInvitationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();
        services.AddSingleton<IToolboxService, ToolboxService>();
        services.AddSingleton<ISystemTokenCredentialService, SystemTokenCredentialService>();
        services.AddScoped<IUserInformationService, UserInformationService>();

        services.AddScoped<IProjectUserManagementService, ProjectUserManagementService>();
        services.AddScoped<IDatabricksApiService, DatabricksApiService>();
        services.AddSingleton<IDatahubCatalogSearch, DatahubCatalogSearch>();

        services.AddSingleton<AzureDevOpsClient>();
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
        services.AddSingleton<IVirusScanStatusListener, VirusScanStatusListener>();
        services.AddMassTransit(x =>
        {
            x.AddConsumer<VirusScanStatusConsumer>();
            x.AddConsumer<HealthCheckConsumer>();            
            x.AddConsumer<HealthCheckResultConsumer>();

            if (DevTools.IsInMemoryServiceBus())
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                    cfg.ReceiveEndpoint(QueueConstants.InfrastructureHealthCheckQueueName,
                        endpoint => { endpoint.Consumer<HealthCheckConsumer>(); });
                    cfg.ReceiveEndpoint(QueueConstants.InfrastructureHealthCheckResultsQueueName,
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
                    cfg.ReceiveEndpoint(QueueConstants.VirusScanStatusQueueName, endpoint => { endpoint.Consumer<VirusScanStatusConsumer>(context); });
                    cfg.ReceiveEndpoint(QueueConstants.InfrastructureHealthCheckQueueName,
                        endpoint => { endpoint.Consumer<HealthCheckConsumer>(); });
                    cfg.ReceiveEndpoint(QueueConstants.InfrastructureHealthCheckResultsQueueName,
                        endpoint => { endpoint.Consumer<HealthCheckResultConsumer>(); });
                });
            }
        });

        return services;
    }

    public static IServiceCollection AddFunctionsInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IQueuePongService, QueuePongService>();
        services.AddScoped<IGCNotifyService, GCNotifyService>();
        services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<IProjectStorageConfigurationService, ProjectStorageConfigurationService>();
        services.AddScoped<IProjectInactivityNotificationService, ProjectInactivityNotificationService>();
        services.AddScoped<IUserInactivityNotificationService, UserInactivityNotificationService>();
        services.AddScoped<IWorkspaceWebAppManagementService, WorkspaceWebAppManagementService>();
        services.AddScoped<IWorkspaceVersionService, WorkspaceVersionService>();
        services.AddScoped<IRequestManagementService, RequestManagementService>();
        services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
        services.AddScoped<IDatahubAuditingService, DatahubTelemetryAuditingService>();
        services.AddScoped<IProjectUserManagementService, ProjectUserManagementService>();
        services.AddScoped<IMSGraphService, MSGraphService>();

        services.AddScoped<ISubnetPoolService, SubnetPoolService>();
        services.AddScoped<IKeyVaultCoreService, KeyVaultCoreService>();

        services.AddSingleton<IWorkspaceBudgetManagementService, WorkspaceBudgetManagementService>();
        services.AddSingleton<IWorkspaceCostManagementService, WorkspaceCostManagementService>();
        services.AddSingleton<IWorkspaceResourceGroupsManagementService, WorkspaceResourceGroupsManagementService>();
        services.AddSingleton<IWorkspaceStorageManagementService, WorkspaceStorageManagementService>();
        services.AddSingleton<IServiceBusConfiguration, NoServiceBusConfiguration>();

        services.AddScoped<HealthCheckHelper>();
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
