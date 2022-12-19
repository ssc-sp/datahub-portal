using System.Text.Json;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ResourceProvisioner.Functions;

public class ResourceRunRequest
{
    private readonly CreateResourceRunCommandHandler _commandHandler;

    public ResourceRunRequest(CreateResourceRunCommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }
    
    [Function("ResourceRunRequest")]
    public async Task RunAsync([QueueTrigger("resource-run-request", Connection = "ResourceRunRequestConnectionString")] string myQueueItem, ILogger log)
    {
        log.LogInformation("C# Queue trigger function started");
        
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var resourceRun = JsonSerializer.Deserialize<CreateResourceRunCommand>(myQueueItem, deserializeOptions);
        
        var resourceRunCommandValidator = new CreateResourceRunCommandValidator();
        var validationResult = await resourceRunCommandValidator.ValidateAsync(resourceRun);
        if (!validationResult.IsValid)
        {
            log.LogError("Validation failed: {Errors}", validationResult.Errors.ToString());
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            await _commandHandler.Handle(resourceRun!, CancellationToken.None);
        }
        catch (Exception e)
        {
            log.LogError(e, "Error while handling resource run request");
            throw;
        }
        log.LogInformation("C# Queue trigger function finished");
    }
}