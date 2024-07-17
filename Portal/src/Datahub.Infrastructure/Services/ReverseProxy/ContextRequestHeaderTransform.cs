using Datahub.Core.Data;
using Microsoft.AspNetCore.Http;
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

        public ContextRequestHeaderTransform(string workspaceAcronym)
        {
            this.workspaceAcronym = workspaceAcronym;
        }

        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            // Access the HttpContext
            var httpContext = context.HttpContext;
            context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

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
