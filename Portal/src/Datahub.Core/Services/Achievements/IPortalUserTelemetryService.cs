namespace Datahub.Core.Services.Achievements;

public interface IPortalUserTelemetryService
{
    /// <summary>
    /// Event raised when new achievements are achieved
    /// </summary>
    event EventHandler<AchievementsEarnedEventArgs> OnAchievementsEarned;
    /// <summary>
    /// Report a telemetry event
    /// </summary>
    /// <param name="eventName">Name, use TelemetryEvents constants</param>
    /// <see cref="TelemetryEvents"/>
    /// <returns></returns>
    Task LogTelemetryEvent(string eventName);
}

public class AchievementsEarnedEventArgs : EventArgs
{
    public List<string> Achievements { get; }
    public bool Hide { get; }

    public AchievementsEarnedEventArgs(List<string> achievements, bool hide = false)
    {
        Achievements = achievements;
        Hide = hide;
    }
}