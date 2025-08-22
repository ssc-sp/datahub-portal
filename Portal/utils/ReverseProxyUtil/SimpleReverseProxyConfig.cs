using Datahub.Application.Configuration;
using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Services.ReverseProxy;
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
            var urlRewritingEnabled = false;
            return [(testAcronym, ReverseProxyRouteHelper.BuildRoute(_config.ReverseProxy.WebAppPrefix, testAcronym, urlRewritingEnabled, true),
                BuildCluster(testAcronym, "http://localhost:8050"))];
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
            return ReverseProxyPathHelper.BuildWebAppURL(_config.ReverseProxy.WebAppPrefix, acronym, routeInfo);
        }

        record ProjectWebData(string Acronym, string Url, bool UrlRewritingEnabled);

    }
}
