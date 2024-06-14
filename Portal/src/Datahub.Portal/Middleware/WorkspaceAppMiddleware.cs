using Datahub.Core.Data;
using Datahub.Infrastructure.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Datahub.Portal.Middleware;

public class WorkspaceAppMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enabled;
    private readonly string _matchPath;
    private static Regex webAppRegEx = new Regex("^/webapp-([A-Z0-9]+)", RegexOptions.Compiled | RegexOptions.Singleline);

    public WorkspaceAppMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _enabled = config.GetValue<bool>("ReverseProxy:Enabled");
        _matchPath = config.GetValue<string>("ReverseProxy:MatchPath") ?? "/webapp";
        
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_enabled)
        {
            var urlPath = context.Request.Path;
            var match = webAppRegEx.Match(urlPath);
            if (match.Success)
            {
                var acronym = match.Groups[1].Value;

                if (!ClaimsContainAcronymRole(context, acronym) && !ClaimsContainAcronymRole(context, RoleConstants.DATAHUB_ADMIN_PROJECT))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;// Status404NotFound;
                    return;
                }
            }
        }
        await _next(context);
    }

    static (string Acronym, bool NeedSlash) InspectWorkspaceAcronym(string urlPath)
    {
        var columns = urlPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var acronym = columns.Length > 1 ? columns[1] : string.Empty;
        var slash = columns.Length == 2 && !urlPath.EndsWith('/');
        return (acronym, slash);
    }

    private bool ClaimsContainAcronymRole(HttpContext context, string acronym)
    {
        if (context.User is null || string.IsNullOrEmpty(acronym))
            return false;

        var wsAppRole = $"{acronym}{RoleConstants.WEBAPP_SUFFIX}";

        return context.User.Claims.Any(c => c.Type == ClaimTypes.Role && wsAppRole.Equals(c.Value, StringComparison.OrdinalIgnoreCase));
    }

    static string GetFullUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}";
    }
}

public static class WorkspaceHttpContextEx
{
    public static string GetWorkspaceRole(this HttpContext context)
    {
        var urlPath = context.Request.Path.ToString();
        var columns = urlPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (columns.Length < 2)
            return string.Empty;

        var acronym = columns[1];
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

        return "authenticated";
    }
}
