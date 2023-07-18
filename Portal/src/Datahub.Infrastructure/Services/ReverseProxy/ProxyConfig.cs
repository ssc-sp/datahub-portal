using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Datahub.Infrastructure.Services.ReverseProxy;

internal class ProxyConfig : IProxyConfig
{
    private readonly IReadOnlyList<RouteConfig> _routes;
    private readonly IReadOnlyList<ClusterConfig> _clusters;

    public ProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        _routes = routes;
        _clusters = clusters;        
    }

    public IReadOnlyList<RouteConfig> Routes => _routes;
    public IReadOnlyList<ClusterConfig> Clusters => _clusters;
    public IChangeToken ChangeToken => new ChangeToken();
}
