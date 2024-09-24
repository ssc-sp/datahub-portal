using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public static class HttpRequestTools
{
    public static string? GetWorkspaceRole(this HttpContext context, string acronym)
    {
        var claims = context.User.Claims.Where(c => c.Type == ClaimTypes.Role);

        var roles = claims.Where(c => c.Value.StartsWith(acronym, StringComparison.OrdinalIgnoreCase))
                          .Select(c => c.Value[acronym.Length..])
                          .ToHashSet();

        if (roles.Contains(RoleConstants.ADMIN_SUFFIX))
            return RoleConstants.ADMIN_ROLE;

        if (roles.Contains(RoleConstants.WORKSPACE_LEAD_SUFFIX))            
            return RoleConstants.WORKSPACE_LEAD_ROLE;

        if (roles.Contains(RoleConstants.COLLABORATOR_SUFFIX))
            return RoleConstants.COLLABORATOR_ROLE;

        if (roles.Contains(RoleConstants.GUEST_SUFFIX))
            return RoleConstants.GUEST_ROLE;

        return null;
    }

    public static bool IsDatahubAdmin(this HttpContext context)
    {
        return GetWorkspaceRole(context, RoleConstants.DATAHUB_ADMIN_PROJECT) != null;
    }
}
