using Datahub.Infrastructure.Queues.Messages;
using MassTransit;
using System.Text.Json;

namespace Datahub.Infrastructure.Services
{
    public interface IHealthCheckConsumer
    {
        Task Consume(ConsumeContext<InfrastructureHealthCheckMessage> context);
    }
    /// <summary>
    /// Local consumer of HealthCheckMessages for debugging and troubleshooting
    /// Saves message to local folder
    /// Service \datahub-portal\Portal\src\Datahub.Infrastructure\Services\Loopback\FileWatcherService.cs consumes the message
    /// and processes it using code copied from \datahub-portal\ServerlessOperations\src\Datahub.Functions\CheckInfrastructureStatus.cs
    /// </summary>
    public class HealthCheckConsumer : IConsumer<InfrastructureHealthCheckMessage>, IHealthCheckConsumer
    {
        public HealthCheckConsumer() { }

        public async Task Consume(ConsumeContext<InfrastructureHealthCheckMessage> context)
        {
            var healthCheckMessage = context.Message; // Get the order details from the message

            // Persist the message to a folder
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "MessageFolder");
            var randomName= Guid.NewGuid().ToString();
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, $"{randomName}.txt");
            var message = JsonSerializer.Serialize(healthCheckMessage);
            await File.WriteAllTextAsync(filePath, message);

            Console.WriteLine($"Got message = {healthCheckMessage}");
        }
    }
} 
