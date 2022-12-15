using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Achievements;

public static class AchievementServiceCollectionExtension
{
    public static IServiceCollection AddAchievementService(
        this IServiceCollection services,
        Action<AchievementServiceOptions> configureOptions)
    {
        services.Configure<AchievementServiceOptions>(configureOptions);
        services.AddScoped<AchievementService>();
        return services;
    }
}

public class AchievementServiceOptions
{
    public bool Enabled { get; set; } = false;
    public string AchievementDirectoryPath { get; set; } = "./Achievements";
    public bool LocalAchievementsOnly { get; set; } = false;
}