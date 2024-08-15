using Datahub.Application.Services.ReverseProxy;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;

namespace Datahub.Infrastructure.Services.ReverseProxy
{
    internal class ContextRequestHeaderTransform : RequestTransform
    {
        private string workspaceAcronym;
        private readonly ILogger logger;

        public ContextRequestHeaderTransform(string workspaceAcronym, ILogger logger)
        {
            this.workspaceAcronym = workspaceAcronym;
            this.logger = logger;
        }

        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            // Access the HttpContext
            var httpContext = context.HttpContext;
            var userRole = httpContext.GetWorkspaceRole(workspaceAcronym);
            var userName = httpContext.User?.Identity?.Name;
            if (userRole is null)
            {
                logger.LogInformation($"WebApp: User {userName} does not have access to workspace {workspaceAcronym}");
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            return default;
        }

        private static bool ClaimsContainAcronymRole(HttpContext context, string acronym)
        {
            if (context.User is null || string.IsNullOrEmpty(acronym))
                return false;

            var wsAppRole = $"{acronym}{RoleConstants.WEBAPP_SUFFIX}";

            return context.User.Claims.Any(c => c.Type == ClaimTypes.Role && wsAppRole.Equals(c.Value, StringComparison.OrdinalIgnoreCase));
        }

    }
}
