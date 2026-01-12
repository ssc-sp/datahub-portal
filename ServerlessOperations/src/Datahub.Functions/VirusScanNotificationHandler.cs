using Azure.Messaging.ServiceBus;
using Datahub.Core.Services.Notification;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

/// <summary>
/// Azure Function that processes virus scan notifications and creates system notifications
/// for users via the existing SystemNotificationService (database-backed, polling-based).
/// </summary>
public class VirusScanNotificationHandler(
    ILogger<VirusScanNotificationHandler> logger,
    ISystemNotificationService systemNotificationService)
{
    [Function("VirusScanNotificationHandler")]
    public async Task RunAsync(
        [ServiceBusTrigger(QueueConstants.VirusScanNotificationQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        logger.LogInformation("Processing virus scan notification: {MessageId}", message.MessageId);

        try
        {
            var notification = await message.DeserializeAndUnwrapMessageAsync<VirusScanNotificationMessage>();

            if (notification == null)
            {
                logger.LogWarning("Failed to deserialize virus scan notification message");
                return;
            }

            logger.LogInformation(
                "Virus scan notification received - Workspace: {Workspace}, File: {FileName}, Status: {Status}, User: {UserObjectId}",
                notification.WorkspaceAcronym,
                notification.FileName,
                notification.ScanStatus,
                notification.UserObjectId);

            // Create system notification using existing notification service
            // This stores notification in database and user will see it via polling
            if (!string.IsNullOrWhiteSpace(notification.UserObjectId))
            {
                var notificationKey = notification.ScanStatus.Equals("Clean", StringComparison.OrdinalIgnoreCase)
                    ? "VIRUS_SCAN.CLEAN_FILE"
                    : "VIRUS_SCAN.INFECTED_FILE";

                await systemNotificationService.CreateSystemNotificationsWithLink(
                    new[] { notification.UserObjectId },
                    $"/w/{notification.WorkspaceAcronym}/data",
                    "VIEW_FILE",
                    notificationKey,
                    notification.FileName,
                    notification.WorkspaceAcronym,
                    notification.ScanCompletedOn.ToString("g"));

                logger.LogInformation(
                    "Created system notification for user {UserId} in workspace {Workspace}",
                    notification.UserObjectId,
                    notification.WorkspaceAcronym);
            }
            else
            {
                logger.LogWarning(
                    "No user object ID provided for notification - skipping user notification");
            }

            logger.LogInformation(
                "Successfully processed virus scan notification for {Workspace}/{FileName}",
                notification.WorkspaceAcronym,
                notification.FileName);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process virus scan notification");
            throw;
        }
    }
}
