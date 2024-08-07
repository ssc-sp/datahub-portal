using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public static class HttpRequestTools
{
    public const string NO_WORKSPACE_ROLE = "authenticated";

    public static string? GetWorkspaceRole(this HttpContext context, string acronym)
    {
        var claims = context.User.Claims.Where(c => c.Type == ClaimTypes.Role);

        var roles = claims.Where(c => c.Value.StartsWith(acronym, StringComparison.OrdinalIgnoreCase))
                          .Select(c => c.Value[acronym.Length..])
                          .ToHashSet();

        if (roles.Contains(RoleConstants.ADMIN_SUFFIX))
            return "admin";

        if (roles.Contains(RoleConstants.WORKSPACE_LEAD_SUFFIX))
            return "lead";

        if (roles.Contains(RoleConstants.COLLABORATOR_SUFFIX))
            return "collaborator";

        if (roles.Contains(RoleConstants.GUEST_SUFFIX))
            return "guest";

        return null;
    }

    public const string USER_HEADER_NAME = "dh-user";

    public static void AddDataHubReverseProxy(this IServiceCollection services)
    {
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
    }
}
