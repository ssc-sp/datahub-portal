using Yarp.ReverseProxy.Configuration;

namespace Datahub.Application.Services.ReverseProxy;

public interface IReverseProxyConfigService
{
	ReverseProxyConfig GetConfigurationFromProjects();

	List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects();
}
public record ReverseProxyConfig(List<RouteConfig> Routes, List<ClusterConfig> Clusters);
