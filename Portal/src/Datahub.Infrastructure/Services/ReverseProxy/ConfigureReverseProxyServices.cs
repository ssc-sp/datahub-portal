using Datahub.Application.Services.ReverseProxy;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public static class ConfigureReverseProxyServices
{
    public const string USER_HEADER_NAME = "dh-user";
    public const string COOKIE_HEADER = "Cookie";

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
                builderContext.AddRequestTransform(async transformContext =>
                {
                    // removing the .AspNetCore cookies from the response
                    var responseHeaders = transformContext.ProxyRequest.Headers;
                    if (responseHeaders.TryGetValues(COOKIE_HEADER, out var cookieValues) && cookieValues is not null)
                    {
                        responseHeaders.Remove(COOKIE_HEADER);
                        var cookies = cookieValues.FirstOrDefault()?.Split(';');
                        if (cookies is not null)
                        {
                            var filteredCookies = cookies.Where(cookie => !cookie.Trim().StartsWith(".AspNetCore")).ToList();
                            if (filteredCookies.Count > 0)
                                responseHeaders.Add(COOKIE_HEADER, string.Join("; ", filteredCookies));
                        }
                    }
                    await Task.CompletedTask;
                });
            });

        return services;
    }
}