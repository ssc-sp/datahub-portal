using System.Security.Claims;

namespace Datahub.Portal.Middleware;

public class WorkspaceAppMiddleware
{
    private readonly RequestDelegate _next;
    public WorkspaceAppMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var urlPath = context.Request.Path;
        if (urlPath.StartsWithSegments("/wsapp", StringComparison.OrdinalIgnoreCase))
        {
            var (acronym, missingSlash) = InspectWorkspaceAcronym(urlPath);

            if (!ClaimsContainAcronymRole(context, acronym))
            {
                context.Response.Redirect(@"/Notfound");
                return;
            }

            if (missingSlash)
            {
                context.Response.Redirect($@"{GetFullUrl(context.Request)}/");
                return;
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
        if (context.User is null)
            return false;

        var adminAcronym = $"{acronym}-admin";

        return context.User.Claims.Any(c => c.Type == ClaimTypes.Role && MatchRole(c.Value, acronym, adminAcronym));
    }

    static bool MatchRole(string claimValue, string role, string adminRole)
    {
        return claimValue.Equals(role, StringComparison.OrdinalIgnoreCase) || claimValue.Equals(adminRole, StringComparison.OrdinalIgnoreCase);
    }

    static string GetFullUrl(HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}{request.Path}";
    }
}
