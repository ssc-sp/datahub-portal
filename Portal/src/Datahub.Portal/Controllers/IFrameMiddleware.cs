namespace Datahub.Portal.Controllers
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AllowIFrameAttribute : Attribute
    {
    }

    public class IFrameMiddleware
    {
        private const string XFrameOptionsHeader = "X-Frame-Options";
        private readonly RequestDelegate _next;
        private readonly ILogger<IFrameMiddleware> logger;

        public IFrameMiddleware(RequestDelegate next, ILogger<IFrameMiddleware> logger)
        {
            _next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var xFrameOptionsValue = "DENY";
            try
            {
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
                    if (controllerActionDescriptor != null)
                    {
                        var methodInfo = controllerActionDescriptor.MethodInfo;
                        var allowIFrame = methodInfo.GetCustomAttributes(typeof(AllowIFrameAttribute), true).Any();

                        if (allowIFrame)
                        {
                            xFrameOptionsValue = "SAMEORIGIN";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Error while processing IFrameMiddleware");
            }

            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(XFrameOptionsHeader))
                {
                    context.Response.Headers[XFrameOptionsHeader] = xFrameOptionsValue;
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
