using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Application.Services.Notebooks;
using Datahub.Application.Services.Notifications;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Announcements;
using Datahub.Infrastructure.Services.CatalogSearch;
using Datahub.Infrastructure.Services.Notebooks;
using Datahub.Infrastructure.Services.Notifications;
using Datahub.Infrastructure.Services.ReverseProxy;
using MediatR;
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
        services.AddSingleton<IResourceMessagingService, ResourceMessagingService>();
        services.AddScoped<IProjectDataRetrievalService, ProjectDataRetrievalService>();
        services.AddScoped<IProjectResourceWhitelistService, ProjectResourcingWhitelistService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IDatahubEmailService, DatahubEmailService>();
        services.AddScoped<IDatabricksApiService, DatabricksApiService>();
        services.AddScoped<IUsersStatusService,UsersStatusService>();
        services.AddSingleton<IDatahubCatalogSearch, DatahubCatalogSearch>();

        if (configuration.GetValue<bool>("ReverseProxy:Enabled"))
        {
            services.AddTransient<IReverseProxyConfigService, ReverseProxyConfigService>();
            services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();
        }
        return services;
    }
}