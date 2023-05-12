using Datahub.Application.Services;
using Datahub.Application.Services.Announcements;
using Datahub.Infrastructure.Queues.MessageHandlers;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Announcements;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddDatahubInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<IUserEnrollmentService, UserEnrollmentService>();
        services.AddScoped<IProjectUserManagementService, ProjectUserManagementService>();
        services.AddSingleton<IResourceRequestService, ResourceRequestService>();
        services.AddScoped<IProjectDataRetrievalService, ProjectDataRetrievalService>();
        services.AddScoped<IProjectResourceWhitelistService, ProjectResourcingWhitelistService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();

        services.AddMediatR(typeof(QueueMessageSender<>));

        return services;
    }
}