using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class HostingServicesFunction
    {
        private readonly ILogger<HostingServicesFunction> _logger;

        public HostingServicesFunction(ILogger<HostingServicesFunction> logger)
        {
            _logger = logger;
        }

        [Function("HostingServicesFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                return new OkObjectResult(requestBody);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error processing request: {e.Message},\n Trace: {e.StackTrace}");
                return new OkObjectResult("Successfully connected, error echoing");
            }
        }
    }
}
