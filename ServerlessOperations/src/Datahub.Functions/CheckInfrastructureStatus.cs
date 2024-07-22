using Azure.Messaging.ServiceBus;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Datahub.Infrastructure.Services.Helpers;
using Microsoft.AspNetCore.Http;

namespace Datahub.Functions;

public class CheckInfrastructureStatus(ILoggerFactory loggerFactory, HealthCheckHelper healthCheckHelper)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<CheckInfrastructureStatus>();

    /// <summary>
    /// Azure Function that runs on a timer to check the infrastructure health of all infrastructure.
    /// </summary>
    /// <param name="timerInfo"></param>
    /// <returns>An OkObjectResult containing the results for all infrastructure tests.</returns>
    [Function("CheckInfrastructureScheduled")]
    public async Task<IActionResult> RunCheckTimer([TimerTrigger("%ProjectUsageCRON%")] TimerInfo timerInfo)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        var results = await healthCheckHelper.RunAllChecks();
        return new OkObjectResult(results);
    }

    /// <summary>
    /// Azure Function that can be called to check a specific infrastructure resource with a POST request.
    /// </summary>
    /// <param name="req"></param>
    /// <returns>An OkObjectResult containing the result for a specific infrastructure test.</returns>
    [Function("CheckInfrastructureStatusHttp")]
    public async Task<IActionResult> RunHealthCheckHttp(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = System.Text.Json.JsonSerializer.Deserialize<InfrastructureHealthCheckMessage>(requestBody);

        return await ProcessRequest(request);
    }

    /// <summary>
    /// Azure Function that consumes messages from the infrastructure health check queue and performs health checks on the specified infrastructure resources.
    /// </summary>
    /// <param name="message">The ServiceBusReceivedMessage containing the infrastructure health check request.</param>
    /// <returns>
    /// An IActionResult containing the results of the health check. If the request group is "all", it returns an OkObjectResult containing the results of all infrastructure checks.
    /// Otherwise, it returns the result of the specific health check associated with the request.
    /// </returns>
    /// <remarks>
    /// The method retrieves the InfrastructureHealthCheckMessage object from the message body and checks the request group. If the group is "all", it calls the RunAllChecks method
    /// to perform health checks on all infrastructure resources. If the group is not "all", it calls the RunHealthCheck method to perform the specific health check associated with
    /// the request. The method logs the processed message using the ILogger instance.
    /// </remarks>
    [Function("CheckInfrastructureStatusQueue")]
    public async Task<IActionResult> RunHealthCheckQueue(
        [ServiceBusTrigger(QueueConstants.InfrastructureHealthCheckQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.Body}");
        
        var request = await message.DeserializeAndUnwrapMessageAsync<InfrastructureHealthCheckMessage>();

        return await ProcessRequest(request);
    }

    private async Task<IActionResult> ProcessRequest(InfrastructureHealthCheckMessage? request)
    {
        if (request is null)
        {
            _logger.LogError("Request could not be deserialized from message.");
            return new BadRequestResult();
        }

        try
        {
            var results = await healthCheckHelper.ProcessHealthCheckRequest(request);

            var bugReportMessages = results.Select(r => healthCheckHelper.CreateBugReportMessage(r.Check));
            await healthCheckHelper.SendBugReportMessagesToQueue(bugReportMessages);

            return new OkObjectResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while running health check");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}