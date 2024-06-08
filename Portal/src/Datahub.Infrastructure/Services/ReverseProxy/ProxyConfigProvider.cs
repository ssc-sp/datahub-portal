using Datahub.Application.Services.ReverseProxy;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public class ProxyConfigProvider : IProxyConfigProvider
{
    private readonly IReverseProxyConfigService _reverseProxyConfigService;
    private CustomMemoryConfig currentConfig = null!;

    public ProxyConfigProvider(IReverseProxyConfigService reverseProxyConfigService)
    {
        _reverseProxyConfigService = reverseProxyConfigService;
        RefreshFromWorkspaces();
    }

    /// <summary>
    /// Note this method the first call could return an empty list of routes and clusters 
    /// then it will be invoked 5 minutes later, and so on.
    /// </summary>
    /// <returns></returns>
    public IProxyConfig GetConfig() => currentConfig;

    //https://blog.fzankl.de/building-a-fast-and-reliable-reverse-proxy-with-yarp
    public void RefreshFromWorkspaces()
    {
        var oldConfig = currentConfig;
        var config = _reverseProxyConfigService.GetConfigurationFromProjects();
        currentConfig = new CustomMemoryConfig(config.Routes, config.Clusters);
        oldConfig?.SignalChange();        
    }

    private class CustomMemoryConfig : IProxyConfig
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public CustomMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        public IReadOnlyList<RouteConfig> Routes { get; }

        public IReadOnlyList<ClusterConfig> Clusters { get; }

        public IChangeToken ChangeToken { get; }

        internal void SignalChange()
        {
            _cts.Cancel();
        }
    }

}



