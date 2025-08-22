using Datahub.Application.Services.ReverseProxy;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public static class ReverseProxyRouteHelper
{
    public static RouteConfig BuildRoute(string webAppPrefix, string acronym, bool urlRewritingEnabled, bool bodyRewritingEnabled)
    {
        var prefix = ReverseProxyPathHelper.BuildWebAppURL(webAppPrefix, acronym, routeInfo: true);

        var route = new RouteConfig()
        {
            RouteId = GetRouteId(acronym),
            ClusterId = GetClusterId(acronym),
            Match = new()
            {
                Path = $"{prefix}/{{**catch-all}}"
            },
            AuthorizationPolicy = IReverseProxyConfigService.WorkspaceAuthorizationPolicy
        };

        var finalRoute = route
            .WithTransformResponseHeader("X-Frame-Options", "SAMEORIGIN", append: false)
            .WithTransformForwarded()
            .WithTransformXForwarded()
            .WithTransform(transform =>
            {
                transform[IReverseProxyConfigService.WorkspaceACLTransform] = acronym;
            });

        if (urlRewritingEnabled)
        {
            if (bodyRewritingEnabled)
            {
                finalRoute = finalRoute.WithTransformRewriteURLsInBody(prefix);
            }
            else
            {
                finalRoute = finalRoute.WithTransformPathRemovePrefix(prefix);
            }
        }

        return finalRoute;
    }

    public static string GetRouteId(string acronym) => $"route-{acronym}".ToLower();
    public static string GetClusterId(string acronym) => $"cluster-{acronym}".ToLower();
}
