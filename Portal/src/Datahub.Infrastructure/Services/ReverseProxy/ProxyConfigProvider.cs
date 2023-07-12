using Datahub.Application.Services.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

namespace Datahub.Infrastructure.Services.ReverseProxy;

internal class ProxyConfigProvider : IProxyConfigProvider
{
    private readonly IReverseProxyConfigService _reverseProxyConfigService;
    private bool _firstCall = true;

    public ProxyConfigProvider(IReverseProxyConfigService reverseProxyConfigService)
    {
        _reverseProxyConfigService = reverseProxyConfigService;
    }

    public IProxyConfig GetConfig()
    {
        if (_firstCall)
        {
            _firstCall = false;
            var routes = new List<RouteConfig>();
            var clusters = new List<ClusterConfig>();
            return new ProxyConfig(routes, clusters);
        }
        else
        {
            var config = _reverseProxyConfigService.GetConfigurationFromProjects();
            return new ProxyConfig(config.Routes, config.Clusters);
        }
    }
}


