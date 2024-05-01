using Datahub.Application.Services.Achievements;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Infrastructure.Services.Achievements;

public static class ConfigureAchievementServices
{
    public static IServiceCollection AddUserAchievementServices(this IServiceCollection services)
    {
        services.AddSingleton<IAchievementEngineFactory, AchievementEngineFactory>();
        services.AddScoped<IPortalUserTelemetryService, PortalUserTelemetryService>();
        return services;
    }
}