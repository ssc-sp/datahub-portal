using Datahub.Core.Data;
using System.Security.Claims;

namespace Datahub.Portal.Middleware;

public class WorkspaceAppMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _enabled;
    private readonly string _matchPath;

    public WorkspaceAppMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _enabled = config.GetValue<bool>("ReverseProxy:Enabled");
        _matchPath = config.GetValue<string>("ReverseProxy:MatchPath") ?? "/wsapp";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_enabled)
        {
            var urlPath = context.Request.Path;
            if (urlPath.StartsWithSegments(_matchPath, StringComparison.OrdinalIgnoreCase))
            {
                var (acronym, missingSlash) = InspectWorkspaceAcronym(urlPath);

                if (!ClaimsContainAcronymRole(context, acronym))
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                if (missingSlash)
                {
                    context.Response.Redirect($@"{GetFullUrl(context.Request)}/");
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
