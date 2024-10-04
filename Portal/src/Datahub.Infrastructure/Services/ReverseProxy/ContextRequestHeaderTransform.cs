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
            var isDatahubAdmin = httpContext.IsDatahubAdmin();
            var userName = httpContext.User?.Identity?.Name;
            if (userRole is null && !isDatahubAdmin)
            {
                logger.LogInformation($"WebApp: User {userName} does not have access to workspace {workspaceAcronym}");
                context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            return default;
        }
    }
}
