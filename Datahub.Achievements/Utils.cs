using System.Text.RegularExpressions;

namespace Datahub.Achievements;

public static class Utils
{
    public static int EventMetricRegexCount(string regexPattern, Dictionary<string, int> visitedUrls)
    {
        var regex = new Regex(regexPattern);
        return visitedUrls
            .Where(kvp => regex.IsMatch(kvp.Key))
            .Sum(kvp => visitedUrls[kvp.Key]);
    }
    
    public static int EventMetricExactCount(string metricName, Dictionary<string, int> eventMetrics)
    {
        return eventMetrics
            .Where(kvp => kvp.Key == metricName)
            .Sum(kvp => eventMetrics[kvp.Key]);
    }
}
