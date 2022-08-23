using System.Text.RegularExpressions;

namespace Datahub.Achievements;

public static class Utils
{
    public static int VisitedUrlCount(string regexPattern, Dictionary<string, int> visitedUrls)
    {
        var regex = new Regex(regexPattern);
        return visitedUrls
            .Where(kvp => regex.IsMatch(kvp.Key))
            .Sum(kvp => visitedUrls[kvp.Key]);
    }
}
