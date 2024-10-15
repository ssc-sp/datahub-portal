using Yarp.ReverseProxy.Configuration;

namespace Datahub.Application.Services.ReverseProxy;

public interface IReverseProxyConfigService
{
    public const string WorkspaceACLTransform = "ACLTransformProvider";
    public const string WorkspaceRouteInfo = "Workspace";
    public const string WorkspaceAuthorizationPolicy = "WorkspaceAppPolicy";
    
    ReverseProxyConfig GetConfigurationFromProjects();

    List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects();

    /// <summary>
    /// Build the web app URL for the given acronym
    /// </summary>
    /// <param name="acronym">Workspace acronym</param>
    /// <param name="routeInfo">the trailing "/" cannot be included when specifying the route info for yarp</param>
    /// <returns>relative path</returns>
    string BuildWebAppURL(string acronym, bool routeInfo = false);
    
    string WebAppPrefix { get; }
}
public record ReverseProxyConfig(List<RouteConfig> Routes, List<ClusterConfig> Clusters);
