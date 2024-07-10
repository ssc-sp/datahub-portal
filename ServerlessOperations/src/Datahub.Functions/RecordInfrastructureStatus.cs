using Azure.Messaging.ServiceBus;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Health;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Datahub.Core.Model.Context;

namespace Datahub.Functions;

public class RecordInfrastructureStatus(
    ILoggerFactory loggerFactory,
    DatahubProjectDBContext dbProjectContext)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RecordInfrastructureStatus>();

    /// <summary>
    /// Azure Function that can be called to record in DB results of heathcheck of a specific infrastructure resource with a POST request.
    /// </summary>
    /// <param name="req"></param>
    /// <returns>An OkResult that means that the heathcheck result was recorded.</returns>
    [Function("RecordInfrastructureStatusHttp")]
    public async Task<IActionResult> RecordHealthCheckHttp(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var request = System.Text.Json.JsonSerializer.Deserialize<InfrastructureHealthCheckResultMessage>(requestBody);
        if (request == null)
        {
            return new BadRequestObjectResult(req.Body);
        }
        return await RecordHealthCheckResults(request);
    }

    /// <summary>
    /// Azure Function that consumes messages from the infrastructure health check results queue and persists it into DB.
    /// </summary>
    /// <param name="message">The ServiceBusReceivedMessage containing the infrastructure health check results.</param>
    /// <returns>
    /// An IActionResult containing the results of the database SaveChanges operation. 
    /// </returns>
    /// <remarks>
    /// The method retrieves the InfrastructureHealthCheckResultMessage object from the message body and persists in DB.
    /// </remarks>
    [Function("RecordInfrastructureStatusQueue")]
    public async Task<IActionResult> RecordHealthCheckQueue(
        [ServiceBusTrigger(QueueConstants.InfrastructureHealthCheckResultsQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.Body}");
        
        var request = await message.DeserializeAndUnwrapMessageAsync<InfrastructureHealthCheckResultMessage>();
        if (request == null)
        {
            return new BadRequestObjectResult(message);
        }
        return await RecordHealthCheckResults(request);
    }

    /// <summary>
    /// Persists in DB the result of an infrastructure health check. 
    /// </summary>
    /// <param name="request">the request containing the resource type, group, name, heathcheck status and details</param>
    /// <returns>An OkResult if operation was successful. A BadResult is returned otherwise.</returns>
    private async Task<IActionResult> RecordHealthCheckResults(InfrastructureHealthCheckResultMessage request)
    {
        try
        {
            await StoreResult(request);          // operational data
            await StoreHealthCheckRun(request);  // historical data

            return new OkResult();
        }
        catch(Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }
    }

    /// <summary>
    /// Function that stores the result of an infrastructure health check in the database.
    /// </summary>
    /// <param name="result">the result of the database SaveChanges operation.</param>
    /// <returns></returns>
    private async Task StoreResult(InfrastructureHealthCheckResultMessage check)
    {
        if (check.Group == null || check.Name == null) { return; }
        var existingChecks = dbProjectContext.InfrastructureHealthChecks.Where(c =>
            c.Group == check.Group && c.Name == check.Name && c.ResourceType == check.ResourceType);

        if (existingChecks != null)
        {
            foreach(var item in existingChecks)
            {
                dbProjectContext.InfrastructureHealthChecks.Remove(item);
            }
        }

        // Add the check without specifying the ID to allow the database to generate it
        dbProjectContext.InfrastructureHealthChecks.Add(new InfrastructureHealthCheck
        {
            Group = check.Group,
            Name = check.Name,
            ResourceType = check.ResourceType,
            Status = check.Status,
            HealthCheckTimeUtc = check.HealthCheckTimeUtc,
            Details = check.Details,
        });
        try
        {
            await dbProjectContext.SaveChangesAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message, check);
            throw;
        }
    }

    /// <summary>
    /// Function that stores the result of an infrastructure health check in the database.
    /// </summary>
    /// <param name="result">the result of the database SaveChanges operation</param>
    /// <returns></returns>
    private async Task StoreHealthCheckRun(InfrastructureHealthCheckResultMessage check)
    {
        if (check.Group == null || check.Name == null) { return; }

        // Add the check run without specifying the ID to allow the database to generate it
        dbProjectContext.InfrastructureHealthCheckRuns.Add(new InfrastructureHealthCheck
        {
            Group = check.Group,
            Name = check.Name,
            ResourceType = check.ResourceType,
            Status = check.Status,
            HealthCheckTimeUtc = check.HealthCheckTimeUtc,
            Details = check.Details,
        });
        try
        {
            await dbProjectContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, check);
        }
    }
}