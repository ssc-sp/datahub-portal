using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MassTransit;

namespace Datahub.Functions;

/// <summary>
/// Azure Function that processes ClamAV completion messages and fans out the resulting
/// email and user-status work onto downstream queues.
/// </summary>
public class VirusScanNotificationHandler(
    ILogger<VirusScanNotificationHandler> logger,
    ISendEndpointProvider sendEndpointProvider,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IUserInformationService userInformationService,
    ILockedUserManagementService lockedUserManagementService)
{
    [Function("VirusScanNotificationHandler")]
    public async Task RunAsync(
        [QueueTrigger(QueueConstants.VirusScanNotificationQueueName,
            Connection = "DatahubStorageQueue:ConnectionString")]
        string message)
    {
        logger.LogInformation("Processing virus scan notification message");
        try
        {
            var notification = await message.DeserializeAndUnwrapMessageAsync<VirusScanNotificationMessage>();
            var scanCompletedOn = notification.ScanCompletedOn.ToString("g");
            var uploader = await ResolveUploaderAsync(notification.UserObjectId);
            var workspaceLead = await ResolveWorkspaceLeadEmailAsync(notification.WorkspaceAcronym);

            logger.LogInformation(
                "Virus scan notification received - Workspace: {Workspace}, File: {FileName}, Status: {Status}, User: {UserObjectId}",
                notification.WorkspaceAcronym,
                notification.FileName,
                notification.ScanStatus,
                notification.UserObjectId);

            if (IsCleanScan(notification.ScanStatus))
            {
                await QueueEmailAsync(new EmailRequestMessage
                {
                    To = CreateRecipientList(uploader?.Email),
                    Subject = $"Virus scan completed for {notification.FileName}",
                    Body = $"<p>Your file <strong>{notification.FileName}</strong> in workspace <strong>{notification.WorkspaceAcronym}</strong> has been scanned successfully at {scanCompletedOn}.</p>",
                });
            }
            else
            {
                await LockExternalUserAsync(notification, uploader);

                if (!string.IsNullOrWhiteSpace(workspaceLead))
                {
                    await QueueEmailAsync(new EmailRequestMessage
                    {
                        To = CreateRecipientList(workspaceLead),
                        Subject = $"Virus scan alert for workspace {notification.WorkspaceAcronym}",
                        Body = $"<p>A file upload in workspace <strong>{notification.WorkspaceAcronym}</strong> did not pass virus scanning.</p><p>File: <strong>{notification.FileName}</strong><br/>Uploaded by: <strong>{uploader?.Email ?? notification.UserObjectId ?? "unknown"}</strong><br/>Status: <strong>{notification.ScanStatus}</strong></p>",
                    });
                }

                await QueueEmailAsync(new EmailRequestMessage
                {
                    To = CreateRecipientList(uploader?.Email),
                    Subject = $"Virus scan failed for {notification.FileName}",
                    Body = $"<p>Your file <strong>{notification.FileName}</strong> in workspace <strong>{notification.WorkspaceAcronym}</strong> did not pass virus scanning. Access remains locked while the issue is reviewed.</p>",
                });
            }

            await QueueUserStatusAsync(new VirusScanUserStatusMessage
            {
                WorkspaceAcronym = notification.WorkspaceAcronym,
                UploaderObjectId = notification.UserObjectId,
                FileName = notification.FileName,
                BlobPath = notification.BlobPath,
                ScanStatus = notification.ScanStatus,
                ScanCompletedOn = notification.ScanCompletedOn,
                FileSizeBytes = notification.FileSizeBytes,
                StorageAccountName = notification.StorageAccountName,
                ContainerName = notification.ContainerName,
                ScanEngine = "ClamAV",
                CorrelationId = notification.CorrelationId,
                AclsApplied = notification.ScanStatus.Equals("Clean", StringComparison.OrdinalIgnoreCase),
            });

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

    private static bool IsCleanScan(string scanStatus)
        => scanStatus.Equals("Clean", StringComparison.OrdinalIgnoreCase);

    private static List<string> CreateRecipientList(string? recipientEmail)
        => string.IsNullOrWhiteSpace(recipientEmail) ? [] : new List<string> { recipientEmail };

    private async Task QueueEmailAsync(EmailRequestMessage emailRequest)
    {
        if (emailRequest.To.Count == 0)
        {
            logger.LogWarning("Skipping email with subject {Subject}: recipient email missing", emailRequest.Subject);
            return;
        }

        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName, emailRequest);
    }

    private async Task<PortalUser?> ResolveUploaderAsync(string? userObjectId)
    {
        if (string.IsNullOrWhiteSpace(userObjectId))
        {
            return null;
        }

        try
        {
            return await userInformationService.GetEntraUserAsync(userObjectId);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to resolve uploader {UserObjectId}.", userObjectId);
            return null;
        }
    }

    private async Task<string?> ResolveWorkspaceLeadEmailAsync(string workspaceAcronym)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.UserRoles)
            .ThenInclude(r => r.PortalUser)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);

        var leadEmail = project?.UserRoles
            .Where(role => role.RoleId == (int)Datahub.Shared.Entities.Project_Role.RoleNames.WorkspaceLead)
            .Select(role => role.PortalUser.Email)
            .FirstOrDefault();

        return leadEmail;
    }

    private async Task LockExternalUserAsync(VirusScanNotificationMessage notification, PortalUser? uploader)
    {
        var portalUserId = uploader?.Id;
        if (portalUserId is null)
        {
            logger.LogWarning(
                "Skipping lock for workspace {Workspace} file {FileName}: uploader portal user could not be resolved.",
                notification.WorkspaceAcronym,
                notification.FileName);
            return;
        }

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = await ctx.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == notification.WorkspaceAcronym);

        await lockedUserManagementService.LockUserAsync(
            portalUserId.Value,
            workspace?.Project_ID,
            $"ClamAV scan result: {notification.ScanStatus}",
            null,
            portalUserId.Value);
    }

    private async Task QueueUserStatusAsync(VirusScanUserStatusMessage userStatusMessage)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.VirusScanUserStatusQueueName, userStatusMessage);
    }
}
