using System.Text.RegularExpressions;
using Datahub.Achievements.Models;
using RulesEngine.Models;

namespace Datahub.Achievements;

public static class Utils
{
    public static int EventMetricRegexCount(string regexPattern, IEnumerable<DatahubTelemetryEventMetric> visitedUrls)
    {
        var regex = new Regex(regexPattern);
        return visitedUrls
            .Where(e => regex.IsMatch(e.Name!))
            .Sum(e => e.Value);
    }
    
    public static int EventMetricExactCount(string metricName, IEnumerable<DatahubTelemetryEventMetric> eventMetrics)
    {
        return eventMetrics
            .Where(e => e.Name == metricName)
            .Sum(e => e.Value);
    }

    public static bool MetaAchievementPassed(string code, IEnumerable<RuleResultTree> ruleResultTrees)
    {
        return ruleResultTrees.Any(r => r.IsSuccess && r.Rule.SuccessEvent == code);
    }
}
