using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class HostingServicesFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<HostingServicesFunction>();
    
    [Function("HostingServicesFunction")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
        HttpRequestData req)
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