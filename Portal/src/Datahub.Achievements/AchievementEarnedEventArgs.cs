using Datahub.Achievements.Models;

namespace Datahub.Achievements;

public class AchievementEarnedEventArgs : EventArgs
{
    public Achievement? Achievement { get; init; }
    public string? UserId { get; init; }
}