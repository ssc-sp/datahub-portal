using System.Text.RegularExpressions;

namespace Datahub.Application.Services.Achievements;

public static class Utils
{
    public static bool MatchUrl(string regexPattern, string currentMetric)
    {
        return Regex.IsMatch(currentMetric, regexPattern);
    }

    public static bool MatchMetric(string metricName, string currentMetric)
    {
        return metricName == currentMetric;
    }

    public static bool OwnsAchievement(string achieventId, HashSet<string> achivements)
    {
        return achivements.Contains(achieventId);
    }
}
