using Datahub.Application.Configuration;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using System.Reflection.Metadata;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
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

    private static string SanitizeWebAppURL(string url)
    {
        if (!url.EndsWith("/"))
        {
            url += "/";
        }
        if (!url.StartsWith("http"))
        {
            url = "https://" + url;
        }
        return url;
    }

    public string BuildWebAppURL(string acronym)
    {
        return IReverseProxyConfigService.WebAppPrefix + "-" + acronym;
    }

    public List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects()
    {       

        var data = _context.Projects
            .Where(e => e.WebAppEnabled == true && e.WebApp_URL != null)
            .Select(e => new ProjectWebData(e.Project_Acronym_CD, SanitizeWebAppURL(e.WebApp_URL), e.WebAppUrlRewritingEnabled))
            .ToList();

        var routes = data.Select(d => BuildRoute(d.Acronym, d.UrlRewritingEnabled)).ToList();
        var clusters = data.Select(d => BuildCluster(d.Acronym, d.Url)).ToList();
        return data.Select(d => (d.Acronym, BuildRoute(d.Acronym, d.UrlRewritingEnabled), BuildCluster(d.Acronym, d.Url))).ToList();
    }

    RouteConfig BuildRoute(string acronym, bool urlRewritingEnabled)
    {
        var prefix = $"/{BuildWebAppURL(acronym)}";
        var route = new RouteConfig()
        {
            RouteId = GetRouteId(acronym),
            ClusterId = GetClusterId(acronym),
            Match = new()
            {
                Path = $"{prefix}/{{**catch-all}}"
            }                        
        };

        var finalRoute = route.
            WithTransformForwarded().
            WithTransformXForwarded();
        if (urlRewritingEnabled)
            finalRoute = finalRoute.WithTransformPathRemovePrefix(prefix);
        return finalRoute;
    }

    static ClusterConfig BuildCluster(string acronym, string webUrl)
    {
        return new ClusterConfig()
        {
            ClusterId = GetClusterId(acronym),
            Destinations = new Dictionary<string, DestinationConfig>()
            {
                { $"destination-{acronym}", new() { Address = webUrl }}
            }
        };
    }

    static string GetRouteId(string acronym) => $"route-{acronym}".ToLower();
    static string GetClusterId(string acronym) => $"cluster-{acronym}".ToLower();

    record ProjectWebData(string Acronym, string Url, bool UrlRewritingEnabled);
}