using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Application.Services.Notebooks;
using Datahub.Application.Services.Notifications;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Announcements;
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
        services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
        services.AddScoped<IProjectUserManagementService, ProjectUserManagementService>();
        services.AddSingleton<IResourceRequestService, ResourceRequestService>();
        services.AddScoped<IProjectDataRetrievalService, ProjectDataRetrievalService>();
        services.AddScoped<IProjectResourceWhitelistService, ProjectResourcingWhitelistService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IDatahubEmailService, DatahubEmailService>();
        services.AddScoped<IDatabricksApiService, DatabricksApiService>();
        services.AddScoped<IUsersStatusService,UsersStatusService>();
        services.AddMediatR(typeof(QueueMessageSender<>));

        if (configuration.GetValue<bool>("ReverseProxy:Enabled"))
        {
            services.AddTransient<IReverseProxyConfigService, ReverseProxyConfigService>();
            services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();
        }
        return services;
    }
}