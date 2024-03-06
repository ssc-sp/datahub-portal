using Yarp.ReverseProxy.Configuration;

namespace Datahub.Application.Services.ReverseProxy;

public interface IReverseProxyConfigService
{
    ReverseProxyConfig GetConfigurationFromProjects();
}

public record ReverseProxyConfig(List<RouteConfig> Routes, List<ClusterConfig> Clusters);
