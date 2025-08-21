using Datahub.Application.Configuration;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace ReverseProxyUtil
{
    public class SimpleReverseProxyConfig : IReverseProxyConfigService
    {
        private readonly DatahubPortalConfiguration _config = new()
        {
            ReverseProxy = new()
            {
                WebAppPrefix = "w"
            }
        };

        public ReverseProxyConfig GetConfigurationFromProjects()
        {
            var allConfig = GetAllConfigurationFromProjects();
            return new ReverseProxyConfig(allConfig.Select(c => c.Route).ToList(), allConfig.Select(c => c.Cluster).ToList());
        }

        public string WebAppPrefix => _config.ReverseProxy.WebAppPrefix;

        public List<(string Acronym, RouteConfig Route, ClusterConfig Cluster)> GetAllConfigurationFromProjects()
        {            
            var testAcronym = "test";
            var urlRewritingEnabled = true;
            return [(testAcronym, BuildRoute(_config.ReverseProxy.WebAppPrefix, testAcronym, urlRewritingEnabled), BuildCluster(testAcronym, "http://localhost:8090"))];
        }

        private static RouteConfig BuildRoute(string webAppPrefix, string acronym, bool urlRewritingEnabled)
        {
            var prefix = ReverseProxyPathHelper.BuildWebAppURL(webAppPrefix, acronym, true);
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
                WithTransformResponseHeader("X-Frame-Options", "SAMEORIGIN", append: false).
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

        public string BuildWebAppURL(string acronym, bool routeInfo = false)
        {
            return ReverseProxyPathHelper.BuildWebAppURL(_config.ReverseProxy.WebAppPrefix, acronym, routeInfo);
        }

        record ProjectWebData(string Acronym, string Url, bool UrlRewritingEnabled);

    }
}
