using Yarp.ReverseProxy.Configuration;

namespace Datahub.Application.Services.ReverseProxy;

public interface IReverseProxyManagerService
{
    IProxyConfigProvider ProxyConfigProvider { get; }

    void ReloadConfiguration();
}