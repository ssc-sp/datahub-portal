using Datahub.Application.Services.ReverseProxy;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public static class ConfigureReverseProxyServices
{
    public const string USER_HEADER_NAME = "dh-user";

    public static IServiceCollection AddDatahubReverseProxyServices(this IServiceCollection services)
    {
        services.AddTransient<IReverseProxyConfigService, ReverseProxyConfigService>();
        services.AddSingleton<IProxyConfigProvider, ProxyConfigProvider>();
        services.AddSingleton<IReverseProxyManagerService, ReverseProxyManagerService>();
        
        services.AddTelemetryConsumer<YarpTelemetryConsumer>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(IReverseProxyConfigService.WorkspaceAuthorizationPolicy, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });
        services.AddReverseProxy()
            .AddTransformFactory<WorkspaceACLTransformFactory>()
            .AddTransforms(builderContext =>
            {
                builderContext.AddXForwarded(ForwardedTransformActions.Append);
                builderContext.AddRequestTransform(async transformContext =>
                {
                    // passing the logged user to the proxied app
                    var loggedUser = transformContext.HttpContext?.User?.Identity?.Name ?? "";
                    transformContext.ProxyRequest.Headers.Add(USER_HEADER_NAME, loggedUser);
                    await Task.CompletedTask;
                });
            });
        
        return services;
    }
}