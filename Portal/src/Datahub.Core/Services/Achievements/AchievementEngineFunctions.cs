namespace Datahub.Core.Services.Achievements;

public static class Utils
{
    public static bool MatchMetric(string metricName, EngineFunctionParms state)
    {
        return metricName == state.CurrentMetric;
    }

    public static bool OwnsAchievement(string achieventId, EngineFunctionParms state)
    {
        return state.CurrentAchivements.Contains(achieventId);
    }
}
