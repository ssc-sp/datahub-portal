using Yarp.ReverseProxy.Configuration;

namespace Datahub.Application.Services.ReverseProxy;

public interface IReverseProxyConfigService
{
    public const string WorkspaceACLTransform = "ACLTransformProvider";
    public const string WorkspaceRouteInfo = "Workspace";
    public const string WorkspaceAuthorizationPolicy = "WorkspaceAppPolicy";
    
    ReverseProxyConfig GetConfigurationFromProjects();

    List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects();

    string BuildWebAppURL(string acronym);
    
    string WebAppPrefix { get; }
}
public record ReverseProxyConfig(List<RouteConfig> Routes, List<ClusterConfig> Clusters);
