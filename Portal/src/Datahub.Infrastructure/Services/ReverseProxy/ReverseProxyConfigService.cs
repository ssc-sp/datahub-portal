using Datahub.Application.Configuration;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Model.Datahub;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

internal class ReverseProxyConfigService : IReverseProxyConfigService
{
	private readonly DatahubProjectDBContext _context;
	private readonly DatahubPortalConfiguration _config;


	public ReverseProxyConfigService(DatahubProjectDBContext context, DatahubPortalConfiguration config)
	{
		_context = context;
		_config = config;
	}

	public ReverseProxyConfig GetConfigurationFromProjects()
	{
		var allConfig = GetAllConfigurationFromProjects();
		return new ReverseProxyConfig(allConfig.Select(c => c.Route).ToList(), allConfig.Select(c => c.Cluster).ToList());

	}

	public List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects()
	{
		var basePath = _config.ReverseProxy.BasePath;

		var data = _context.Projects
			.Where(e => e.WebAppEnabled == true && e.WebAppURL != null)
			.Select(e => new ProjectWebData(e.ProjectAcronymCD, e.WebAppURL))
			.ToList();

		var routes = data.Select(d => BuildRoute(basePath, d.Acronym)).ToList();
		var clusters = data.Select(d => BuildCluster(d.Acronym, d.Url)).ToList();
		return data.Select(d => (d.Acronym, BuildRoute(basePath, d.Acronym), BuildCluster(d.Acronym, d.Url))).ToList();
	}

	static RouteConfig BuildRoute(string basePath, string acronym)
	{
		var prefix = $"/{basePath}/{acronym}";
		var route = new RouteConfig()
		{
			RouteId = GetRouteId(acronym),
			ClusterId = GetClusterId(acronym),
			Match = new()
			{
				Path = $"{prefix}/{{**catch-all}}"
			}
		};
		return route.WithTransformPathRemovePrefix(prefix);
	}

	static ClusterConfig BuildCluster(string acronym, string webUrl)
	{
		return new ClusterConfig()
		{
			ClusterId = GetClusterId(acronym),
			Destinations = new Dictionary<string, DestinationConfig>()
			{
				{ "destination1", new() { Address = webUrl }}
			}
		};
	}

	static string GetRouteId(string acronym) => $"route-{acronym}".ToLower();
	static string GetClusterId(string acronym) => $"cluster-{acronym}".ToLower();

	record ProjectWebData(string Acronym, string Url);
}