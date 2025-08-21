using Datahub.Application.Services.ReverseProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Datahub.Infrastructure
{
    public static class ReverseProxyUrlRewriteExtensions
    {
        /// <summary>
        /// Adds the URL rewrite middleware to the pipeline, before MapReverseProxy.
        /// </summary>
        public static IApplicationBuilder UseReverseProxyUrlRewriter(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Services.ReverseProxy.ReverseProxyUrlRewriteMiddleware>();
        }
    }
}
