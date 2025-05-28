using System;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues.Models;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.WorkspaceVersion.Commands.WorkspaceVersionUpdate;

namespace ResourceProvisioner.Functions
{
    public class WorkspaceVersionUpdateRequest(ILoggerFactory loggerFactory)
    {
        private readonly ILogger<ResourceRunRequest> _logger = loggerFactory.CreateLogger<ResourceRunRequest>();

        [Function("WorkspaceVersionUpdateRequest")]
        public async Task RunAsync([ServiceBusTrigger(QueueConstants.WorkspaceVersionUpdateRequestQueueName, Connection = "DatahubServiceBus:ConnectionString")] ServiceBusReceivedMessage myQueueItem)
        {
            _logger.LogInformation("C# Workspace Version Update Request Queue trigger function started");

            var deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var messageEnvelope = await JsonDocument.ParseAsync(myQueueItem.Body.ToStream());
            messageEnvelope.RootElement.TryGetProperty("message", out var message);
            var projectIdsToUpdate = message.Deserialize<WorkspaceVersionUpdateCommand>(deserializeOptions);


            _logger.LogInformation("C# Workspace Version Update Request Queue trigger function ended");
        }
    }
}
