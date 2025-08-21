namespace Datahub.Application.Services.ReverseProxy;

public static class ReverseProxyPathHelper
{
    /// <summary>
    /// Build the web app URL for the given acronym.
    /// </summary>
    /// <param name="prefix">Workspace prefix, e.g. "w"</param>
    /// <param name="acronym">Workspace acronym</param>
    /// <param name="routeInfo">the trailing "/" cannot be included when specifying the route info for yarp</param>
    /// <returns>relative path</returns>
    public static string BuildWebAppURL(string prefix, string acronym, bool routeInfo = false)
        => "/" + prefix + "/" + acronym + (routeInfo ? "" : "/");
}
