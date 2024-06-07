using Amazon.Runtime.Internal.Util;
using Datahub.Application.Services.ReverseProxy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public class ReverseProxyManagerService : IReverseProxyManagerService
{
    private readonly ILogger<ReverseProxyManagerService> logger;

    public ReverseProxyManagerService(IProxyConfigProvider proxyConfigProvider, ILogger<ReverseProxyManagerService> logger)
    {
        ProxyConfigProvider = proxyConfigProvider;
        this.logger = logger;
    }

    public IProxyConfigProvider ProxyConfigProvider { get; }

    private ProxyConfigProvider? InternalProxyConfigProvider
    {
        get
        {
            return ProxyConfigProvider as ProxyConfigProvider;
        }
    }

    public void ReloadConfiguration()
    {
        logger.LogInformation($"Reloading Yarp reverse proxy configuration");
        InternalProxyConfigProvider?.RefreshFromWorkspaces();
    }
}
