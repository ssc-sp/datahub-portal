using Datahub.Infrastructure.Queues.Messages;
using MassTransit;
using System.Text.Json;

namespace Datahub.Infrastructure.Services
{
    public interface IHealthCheckResultConsumer
    {
        Task Consume(ConsumeContext<InfrastructureHealthCheckResultMessage> context);
    }
    /// <summary>
    /// Local consumer of InfrastructureHealthCheckResultMessage for debugging and troubleshooting
    /// Saves message to local folder
    /// Service \datahub-portal\Portal\src\Datahub.Infrastructure\Services\Loopback\FileWatcherService.cs consumes the message
    /// and processes it using code copied from \datahub-portal\ServerlessOperations\src\Datahub.Functions\RecordInfrastructureStatus.cs
    /// </summary>
    public class HealthCheckResultConsumer : IConsumer<InfrastructureHealthCheckResultMessage>, IHealthCheckResultConsumer
    {
        public HealthCheckResultConsumer() { }

        public async Task Consume(ConsumeContext<InfrastructureHealthCheckResultMessage> context)
        {
            var healthCheckResultMessage = context.Message; // Get the order details from the message

            // Persist the message to a folder
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "MessageFolder");
            var randomName= Guid.NewGuid().ToString();
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, $"{randomName}.txt");
            var message = JsonSerializer.Serialize(healthCheckResultMessage);
            await File.WriteAllTextAsync(filePath, message);

            Console.WriteLine($"Got message = {healthCheckResultMessage}");
        }
    }
} 
