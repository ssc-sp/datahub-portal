using Datahub.Application.Configuration;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

internal class ReverseProxyConfigService : IReverseProxyConfigService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly DatahubPortalConfiguration _config;


    public ReverseProxyConfigService(IDbContextFactory<DatahubProjectDBContext> contextFactory, DatahubPortalConfiguration config)
    {
        _contextFactory = contextFactory;
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


    /// <summary>
    /// Build the web app URL for the given acronym
    /// </summary>
    /// <param name="prefix">Workspace prefix, e.g. "w"</param>
    /// <param name="acronym">Workspace acronym</param>
    /// <param name="routeInfo">the trailing "/" cannot be included when specifying the route info for yarp</param>
    /// <returns>relative path</returns>

    private static string BuildWebAppURL(string prefix, string acronym, bool routeInfo = false)
    {
        return ReverseProxyPathHelper.BuildWebAppURL(prefix, acronym, routeInfo);
    }

    public string WebAppPrefix => _config.ReverseProxy.WebAppPrefix;

    public List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects()
    {       
        using var ctx = _contextFactory.CreateDbContext();
        var data = ctx.Projects
            .Where(e => e.WebAppEnabled == true && e.WebApp_URL != null)
            .Select(e => new ProjectWebData(e.Project_Acronym_CD, SanitizeWebAppURL(e.WebApp_URL), e.WebAppUrlRewritingEnabled))
            .ToList();

        return data.Select(d => (
            d.Acronym,
            ReverseProxyRouteHelper.BuildRoute(_config.ReverseProxy.WebAppPrefix, d.Acronym, d.UrlRewritingEnabled, d.UrlRewritingEnabled),
            BuildCluster(d.Acronym, d.Url)
        )).ToList();
    }

    static ClusterConfig BuildCluster(string acronym, string webUrl)
    {
        return new ClusterConfig()
        {
            ClusterId = ReverseProxyRouteHelper.GetClusterId(acronym),
            Destinations = new Dictionary<string, DestinationConfig>()
            {
                { $"destination-{acronym}", new() { Address = webUrl }}
            }
        };
    }

    public string BuildWebAppURL(string acronym, bool routeInfo = false)
    {
        return BuildWebAppURL(_config.ReverseProxy.WebAppPrefix, acronym, routeInfo);
    }

    record ProjectWebData(string Acronym, string Url, bool UrlRewritingEnabled);
}