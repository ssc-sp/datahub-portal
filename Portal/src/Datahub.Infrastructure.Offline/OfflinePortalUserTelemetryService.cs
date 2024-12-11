using Datahub.Application.Services.Achievements;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Offline;

public class OfflinePortalUserTelemetryService : IPortalUserTelemetryService
{
    public event EventHandler<AchievementsEarnedEventArgs>? OnAchievementsEarned;

    private readonly ILogger<OfflinePortalUserTelemetryService> _logger;

    public OfflinePortalUserTelemetryService(ILogger<OfflinePortalUserTelemetryService> logger)
    {
        _logger = logger;
    }

    public DateTime GetUserLastLogin()
    {
        return new DateTime();
    }

    public Task FindUserLastLogin()
    {
        _logger.LogWarning("Error.");
        return Task.CompletedTask;
    }

    public Task LogTelemetryEvent(string eventName)
    {
        _logger.LogInformation("Logging Telemetry Event: {EventName}", eventName);
        return Task.CompletedTask;
    }
}