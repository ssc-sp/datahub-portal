using System.Text.Json;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ResourceProvisioner.Functions;

public class ResourceRunRequest
{
    private readonly IConfiguration _configuration;
    private readonly CreateResourceRunCommandHandler _commandHandler;
    private readonly ILogger<ResourceRunRequest> _logger;

    public ResourceRunRequest(ILoggerFactory loggerFactory, IConfiguration configuration, CreateResourceRunCommandHandler commandHandler)
    {
        _logger = loggerFactory.CreateLogger<ResourceRunRequest>();
        _configuration = configuration;
        _commandHandler = commandHandler;
    }

    [Function("ResourceRunRequest")]
    public async Task RunAsync([QueueTrigger("resource-run-request", Connection = "ResourceRunRequestConnectionString")] string myQueueItem)
    {
        _logger.LogInformation("C# Queue trigger function started");

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var resourceRun = JsonSerializer.Deserialize<CreateResourceRunCommand>(myQueueItem, deserializeOptions);

        var resourceRunCommandValidator = new CreateResourceRunCommandValidator();
        var validationResult = await resourceRunCommandValidator.ValidateAsync(resourceRun);
        if (!validationResult.IsValid)
        {
            _logger.LogError("Validation failed: {Errors}", validationResult.Errors.ToString());
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            await _commandHandler.Handle(resourceRun!, CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while handling resource run request");
            throw;
        }
        _logger.LogInformation("C# Queue trigger function finished");
    }
}