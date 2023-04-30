using System.Collections.Generic;

namespace Datahub.Core.Services.Achievements;

public static class Utils
{
    public static bool MatchMetric(string metricName, string currentMetric)
    {
        return metricName == currentMetric;
    }

    public static bool OwnsAchievement(string achieventId, HashSet<string> achivements)
    {
        return achivements.Contains(achieventId);
    }
}
