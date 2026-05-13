using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Datahub.Shared.Configuration;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Services;
using Datahub.Shared.Entities;

namespace ResourceProvisioner.Functions;

public class ResourceRunRequest(ILoggerFactory loggerFactory, IRepositoryService repositoryService)
{
    private readonly ILogger<ResourceRunRequest> _logger = loggerFactory.CreateLogger<ResourceRunRequest>();

    [Function("ResourceRunRequest")]
    public async Task RunAsync(
        [ServiceBusTrigger(QueueConstants.ResourceRunRequestQueueName, Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage myQueueItem)
    {
        _logger.LogInformation("C# Queue trigger function started");
        
        _logger.LogInformation("Processing ServiceBus message - MessageId: {MessageId}, DeliveryCount: {DeliveryCount}, EnqueuedTime: {EnqueuedTime}", 
            myQueueItem.MessageId, myQueueItem.DeliveryCount, myQueueItem.EnqueuedTime);


        _logger.LogInformation("ServiceBus message body: {MessageBody}", myQueueItem.Body.ToString());

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        var messageEnvelope = await JsonDocument.ParseAsync(myQueueItem.Body.ToStream());
        messageEnvelope.RootElement.TryGetProperty("message", out var message);
        var resourceRun = message.Deserialize<WorkspaceDefinition>(deserializeOptions);
        
        var resourceRunCommandValidator = new WorkspaceDefinitionValidator();
        var validationResult = await resourceRunCommandValidator.ValidateAsync(resourceRun!);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed: {Errors}", validationResult.Errors.ToString());
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            var pullRequestMessage = await repositoryService.HandleResourcing(resourceRun!);
            _logger.LogInformation("Resource run request processed successfully {PullRequestMessage}", pullRequestMessage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while handling resource run request");
            throw;
        }
        _logger.LogInformation("C# Queue trigger function finished");
    }
}
