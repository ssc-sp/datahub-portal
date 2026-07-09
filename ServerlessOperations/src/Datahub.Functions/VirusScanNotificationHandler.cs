using Azure.Storage.Blobs;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Datahub.Functions;

/// <summary>
/// Processes minimal ClamAV scan result messages by enriching with workspace/user context,
/// sending notification emails, locking users on failed scans, and queuing downstream status updates.
/// </summary>
public class VirusScanNotificationHandler(
    ILogger<VirusScanNotificationHandler> logger,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IConfiguration configuration,
    ISystemTokenCredentialService tokenCredentialService,
    ILockedUserManagementService lockedUserManagementService,
    IGCNotifyService gcNotifyService)
{

    [Function("VirusScanNotificationHandler")]
    public async Task RunAsync(
        [QueueTrigger(StorageQueueConstants.ClamAvScanResultQueueName,
            Connection = "DatahubStorageQueue:ConnectionString")]
        string message)
    {
        logger.LogInformation("Processing ClamAV scan result message");
        try
        {
            var scanResult = JsonSerializer.Deserialize<ClamAvScanResultMessage>(message);
            if (scanResult == null)
            {
                logger.LogWarning("Failed to deserialize ClamAV scan result message");
                return;
            }

            var connectionString = configuration.GetConnectionString("DatahubStorageQueue:ConnectionString");
            var storageAccountName = ExtractStorageAccountName(connectionString);
            if (string.IsNullOrEmpty(storageAccountName))
            {
                logger.LogError("Failed to extract storage account name from connection string");
                return;
            }

            var workspaceAcronym = await ResolveWorkspaceAcronymAsync(storageAccountName) ?? storageAccountName;

            var blobClient = new BlobClient(new Uri(scanResult.ScannedFile), tokenCredentialService.GetTokenCredential());
            var properties = await blobClient.GetPropertiesAsync();
            var metadata = properties.Value.Metadata;

            var scanStatus = DetermineScanStatus(scanResult.ScanError, metadata);
            var userId = metadata.TryGetValue(FileMetadata.CreatedBy, out var uploaderId)
                ? uploaderId
                : ExtractUserFromPath(scanResult.ScannedFile);

            var notification = new VirusScanNotificationMessage
            {
                WorkspaceAcronym = workspaceAcronym,
                UserId = userId,
                FileName = Path.GetFileName(scanResult.ScannedFile),
                BlobPath = scanResult.ScannedFile,
                ScanStatus = scanStatus,
                ScanCompletedOn = scanResult.ScanEndTime,
                FileSizeBytes = properties.Value.ContentLength,
                StorageAccountName = storageAccountName,
                CorrelationId = Guid.NewGuid().ToString()
            };

            await ProcessNotificationAsync(notification);

            logger.LogInformation(
                "Successfully processed virus scan result for {Workspace}/{FileName} with status {Status}",
                notification.WorkspaceAcronym,
                notification.FileName,
                notification.ScanStatus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process ClamAV scan result");
            throw;
        }
    }

    private async Task ProcessNotificationAsync(VirusScanNotificationMessage notification)
    {
        var scanCompletedOn = notification.ScanCompletedOn.ToString("g");
        var workspaceLead = await ResolveWorkspaceLeadEmailAsync(notification.WorkspaceAcronym);

        if (!IsCleanScan(notification.ScanStatus))
        {
            await LockExternalUserAsync(notification, notification.UserId);

            if (!string.IsNullOrWhiteSpace(workspaceLead))
            {
                await gcNotifyService.SendInfectedFileNotification(
                    workspaceLead,
                    notification.FileName,
                    notification.WorkspaceAcronym,
                    scanCompletedOn);
            }

            await gcNotifyService.SendInfectedFileNotification(
                notification.UserId,
                notification.FileName,
                notification.WorkspaceAcronym,
                scanCompletedOn);
        }

        await QueueUserStatusAsync(new VirusScanStatusMessage
        {
            WorkspaceAcronym = notification.WorkspaceAcronym,
            UploaderObjectId = notification.UserId,
            FileName = notification.FileName,
            BlobPath = notification.BlobPath,
            ScanStatus = notification.ScanStatus,
            ScanCompletedOn = notification.ScanCompletedOn,
            FileSizeBytes = notification.FileSizeBytes,
            StorageAccountName = notification.StorageAccountName,
            ScanEngine = "ClamAV",
            CorrelationId = notification.CorrelationId,
        });
    }

    private static bool IsCleanScan(ScanStatusType scanStatus)
        => scanStatus == ScanStatusType.Clean;

    private static List<string> CreateRecipientList(string? recipientEmail)
        => string.IsNullOrWhiteSpace(recipientEmail) ? [] : [recipientEmail];

    private async Task<string?> ResolveWorkspaceLeadEmailAsync(string workspaceAcronym)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.UserRoles)
            .ThenInclude(r => r.PortalUser)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);

        return project?.UserRoles
            .Where(role => role.RoleId == (int)Project_Role.RoleNames.WorkspaceLead)
            .Select(role => role.PortalUser.Email)
            .FirstOrDefault();
    }

    private async Task LockExternalUserAsync(VirusScanNotificationMessage notification, string uploader)
    {
        var details = $"File is {notification.FileName}, ClamAV scan result {notification.ScanStatus}, Workspace {notification.WorkspaceAcronym}, storage {notification.StorageAccountName}";
        using var ctx = await dbContextFactory.CreateDbContextAsync();
        var portalUser = await ctx.PortalUsers.FirstOrDefaultAsync(u => string.Equals(u.Email, uploader, StringComparison.CurrentCultureIgnoreCase));
        if (portalUser is null)
            await gcNotifyService.SendDataHubErrorNotification($"Virus Detected but cannot lock user '{uploader}'. {details}");
        else
            await lockedUserManagementService.LockUserAsync(
                portalUser.Id,
                $"{details}",
                null,
                null);
    }

    private async Task QueueUserStatusAsync(VirusScanStatusMessage userStatusMessage)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.VirusScanStatusQueueName, userStatusMessage);
    }

    private static string? ExtractStorageAccountName(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return null;

        var match = Regex.Match(connectionString, @"AccountName=([^;]+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private async Task<string?> ResolveWorkspaceAcronymAsync(string storageAccountName)
    {
        try
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();

            var project = await ctx.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Resources.Any(r =>
                    r.ResourceType == "StorageAccount" &&
                    r.JsonContent != null &&
                    r.JsonContent.Contains(storageAccountName, StringComparison.OrdinalIgnoreCase)));

            return project?.Project_Acronym_CD;
        }
        catch (DbException ex)
        {
            logger.LogWarning(ex, "Failed to resolve workspace for storage account {StorageAccount}", storageAccountName);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Failed to resolve workspace for storage account {StorageAccount}", storageAccountName);
            return null;
        }
    }

    private static string? ExtractUserFromPath(string blobPath)
    {
        var match = Regex.Match(blobPath, @"^(?:upload|shared)/([^/]+)/", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static ScanStatusType DetermineScanStatus(string scanError, IDictionary<string, string> metadata)
    {
        if (!string.IsNullOrEmpty(scanError))
            return ScanStatusType.Failed;

        if (metadata.TryGetValue(FileMetadata.AvScan, out var avScanResult))
        {
            return avScanResult.Equals("ok", StringComparison.OrdinalIgnoreCase)
                ? ScanStatusType.Clean
                : ScanStatusType.Infected;
        }

        return ScanStatusType.Failed;
    }

    private record ClamAvScanResultMessage
    {
        public DateTimeOffset ScanStartTime { get; init; }
        public DateTimeOffset ScanEndTime { get; init; }
        public string ScanError { get; init; } = string.Empty;
        public string ScannedFile { get; init; } = string.Empty;
    }
}
