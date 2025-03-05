namespace Datahub.Portal.Controllers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AllowIFrameAttribute : Attribute
    {
    }

    public class IFrameMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IFrameMiddleware> logger;

        public IFrameMiddleware(RequestDelegate next, ILogger<IFrameMiddleware> logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            bool headerConfigured = false;
            try
            {
                if (endpoint != null)
                {
                    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
                    if (controllerActionDescriptor != null)
                    {
                        var methodInfo = controllerActionDescriptor.MethodInfo;
                        var allowIFrame = methodInfo.GetCustomAttributes(typeof(AllowIFrameAttribute), true).Any();

                        // Add the header only if the AllowIFrameAttribute is not present
                        if (allowIFrame)
                        {
                            context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                            headerConfigured = true;
                        }
                    }
                }
            } catch (Exception e)
            {
                logger.LogWarning(e, "Error while processing IFrameMiddleware");
            }
            if (!headerConfigured)
            {
                context.Response.Headers.Append("X-Frame-Options", "DENY");
            }

            await _next(context);
        }
    }


}
