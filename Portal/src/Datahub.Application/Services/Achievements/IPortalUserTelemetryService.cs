namespace Datahub.Application.Services.Achievements;

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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