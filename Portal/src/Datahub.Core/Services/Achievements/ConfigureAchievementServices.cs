using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Core.Services.Achievements;

public static class ConfigureAchievementServices
{
    public static IServiceCollection AddUserAchievementServices(this IServiceCollection services)
    {
        services.AddSingleton<IAchievementEngineFactory, AchievementEngineFactory>();
        services.AddScoped<IPortalUserTelemetryService, PortalUserTelemetryService>();
        return services;
    }
}