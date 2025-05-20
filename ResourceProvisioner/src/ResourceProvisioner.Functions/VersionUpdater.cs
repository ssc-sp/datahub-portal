using System;
using Azure.Messaging.ServiceBus;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ResourceProvisioner.Functions
{
    public class VersionUpdater(ILoggerFactory loggerFactory)
    {
        private readonly ILogger<ResourceRunRequest> _logger = loggerFactory.CreateLogger<ResourceRunRequest>();

        [Function(nameof(VersionUpdater))]
        public void Run([ServiceBusTrigger(QueueConstants.VersionUpdaterQueueName, Connection = "DatahubServiceBus:ConnectionString")] ServiceBusReceivedMessage message)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        }
    }
}
