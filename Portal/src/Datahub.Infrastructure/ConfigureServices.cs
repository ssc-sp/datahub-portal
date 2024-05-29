﻿using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Application.Services.Notebooks;
using Datahub.Application.Services.Notifications;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Announcements;
using Datahub.Infrastructure.Services.CatalogSearch;
using Datahub.Infrastructure.Services.Notebooks;
using Datahub.Infrastructure.Services.Notifications;
using Datahub.Infrastructure.Services.ReverseProxy;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Infrastructure.Services.Subscriptions;
using Datahub.Infrastructure.Services.UserManagement;
using MassTransit;
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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Datahub.Infrastructure.ConfigureServices).Assembly));
        services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
        services.AddScoped<IProjectUserManagementService, ProjectUserManagementService>();
        services.AddScoped<IProjectStorageConfigurationService, ProjectStorageConfigurationService>();
        services.AddScoped<CloudStorageManagerFactory>();
        services.AddScoped<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<IProjectResourceWhitelistService, ProjectResourcingWhitelistService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IDatahubEmailService, DatahubEmailService>();
        services.AddScoped<IDatabricksApiService, DatabricksApiService>();
        services.AddScoped<IUsersStatusService,UsersStatusService>();
        services.AddSingleton<IDatahubCatalogSearch, DatahubCatalogSearch>();
        services.AddScoped<IDatahubAzureSubscriptionService, DatahubAzureSubscriptionService>();
        services.AddScoped<IUserInformationService, UserInformationService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();

        if (configuration.GetValue<bool>("ReverseProxy:Enabled"))
        {
            services.AddTransient<IReverseProxyConfigService, ReverseProxyConfigService>();
            services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();
        }

        services.AddMassTransit(x =>
        {
            x.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(configuration["DatahubServiceBus:ConnectionString"]);
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}