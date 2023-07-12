using Datahub.Application.Services.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

namespace Datahub.Infrastructure.Services.ReverseProxy;

internal class ProxyConfigProvider : IProxyConfigProvider
{
    private readonly IReverseProxyConfigService _reverseProxyConfigService;

    public ProxyConfigProvider(IReverseProxyConfigService reverseProxyConfigService)
    {
        _reverseProxyConfigService = reverseProxyConfigService;
    }

    /// <summary>
    /// Note this method the first call could return an empty list of routes and clusters 
    /// then it will be invoked 5 minutes later, and so on.
    /// </summary>
    /// <returns></returns>
    public IProxyConfig GetConfig()
    {
        var config = _reverseProxyConfigService.GetConfigurationFromProjects();
        return new ProxyConfig(config.Routes, config.Clusters);
    }
}


