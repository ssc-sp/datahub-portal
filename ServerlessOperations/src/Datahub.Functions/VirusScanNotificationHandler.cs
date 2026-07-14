using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Datahub.Functions;

public class VirusScanNotificationHandler(
    ILogger<VirusScanNotificationHandler> logger,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IConfiguration configuration,
    ILockedUserManagementService lockedUserManagementService,
    IGCNotifyService gcNotifyService,
    IWorkspaceStorageManagementService workspaceStorageManagementService)
{
    [Function("ClamAVNotificationHandler")]
    public async Task RunAsync(
        [ServiceBusTrigger(QueueConstants.TerraformOutputHandlerQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        logger.LogInformation("Processing ClamAV scan result message");
        try
        {
            var scanResult = await message.DeserializeAndUnwrapMessageAsync<ClamAVMessage>();

            var connectionString = configuration.GetConnectionString("DatahubStorageQueue:ConnectionString");
            var storageAccountName = ExtractStorageAccountName(connectionString);
            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                logger.LogWarning("Failed to extract storage account name from connection string");
            }

            var workspaceAcronym = !string.IsNullOrWhiteSpace(storageAccountName)
                ? await ResolveWorkspaceAcronymAsync(storageAccountName) ?? storageAccountName
                : "unknown";
            var scanStatus = DetermineScanStatus(scanResult.ScanError);
            var fileName = Path.GetFileName(scanResult.ScannedFile);
            var blobPath = scanResult.ScannedFile;
            var uploader = !string.IsNullOrWhiteSpace(scanResult.OriginalBlobMetadata?.CreatedBy)
                ? scanResult.OriginalBlobMetadata.CreatedBy
                : ExtractUserFromPath(scanResult.ScannedFile);
            var correlationId = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(scanResult.ScanError))
            {
                var targetBlobPath = await workspaceStorageManagementService.MoveBlobToUsersContainerAsync(scanResult.ScannedFile, connectionString);

                await QueueUserStatusAsync(new VirusScanStatusMessage
                {
                    WorkspaceAcronym = workspaceAcronym,
                    UploaderObjectId = uploader,
                    FileName = fileName,
                    BlobPath = targetBlobPath,
                    ScanStatus = scanStatus,
                    ScanCompletedOn = scanResult.ScanEndTime,
                    FileSizeBytes = null,
                    StorageAccountName = storageAccountName,
                    ScanEngine = "ClamAV",
                    CorrelationId = correlationId,
                });

                logger.LogInformation(
                    "Forwarded virus scan status for {Workspace}/{FileName} to {QueueName}",
                    workspaceAcronym,
                    fileName,
                    QueueConstants.VirusScanStatusQueueName);
            }
            else
            {
                var scanCompletedOn = scanResult.ScanEndTime.ToString("g");
                await gcNotifyService.SendInfectedFileNotification(
                    IGCNotifyService.DEFAULT_MAILBOX,
                    fileName,
                    workspaceAcronym,
                    scanCompletedOn);

                var owner = await ResolveWorkspaceLeadEmailAsync(workspaceAcronym);
                if (!string.IsNullOrWhiteSpace(owner))
                {
                    await gcNotifyService.SendInfectedFileNotification(
                        owner,
                        fileName,
                        workspaceAcronym,
                        scanCompletedOn);
                }

                if (string.IsNullOrWhiteSpace(uploader))
                {
                    logger.LogWarning(
                        "ClamAV scan error reported for {FileName} but uploader is missing. ScanError: {ScanError}",
                        fileName,
                        scanResult.ScanError);
                }
                else
                {

                    await LockExternalUserAsync(fileName, workspaceAcronym, storageAccountName, scanStatus, uploader);

                    logger.LogInformation(
                        "Blocked user {User} due to ClamAV scan error for {Workspace}/{FileName}",
                        uploader,
                        workspaceAcronym,
                        fileName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process ClamAV scan result");
            throw;
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

        return project?.UserRoles
            .Where(role => role.RoleId == (int)Project_Role.RoleNames.WorkspaceLead)
            .Select(role => role.PortalUser.Email)
            .FirstOrDefault();
    }

    private async Task LockExternalUserAsync(string fileName, string workspaceAcronym, string? storageAccountName, ScanStatusType scanStatus, string uploader)
    {
        var details = $"File is {fileName}, ClamAV scan result {scanStatus}, Workspace {workspaceAcronym}, storage {storageAccountName ?? "unknown"}";
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var portalUser = await ctx.EntraUsers
            .AsNoTracking()
            .Where(e => e.GraphGuid == uploader)
            .Select(e => e.PortalUser)
            .FirstOrDefaultAsync();

        if (portalUser is null)
        {
            portalUser = await ctx.PortalUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => string.Equals(u.Email, uploader, StringComparison.CurrentCultureIgnoreCase));
        }

        if (portalUser is null)
        {
            await gcNotifyService.SendDataHubErrorNotification($"Virus Detected but cannot lock user '{uploader}'. {details}");
            return;
        }

        await lockedUserManagementService.LockUserAsync(
            portalUser.Id,
            details,
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
        if (string.IsNullOrWhiteSpace(blobPath))
            return null;

        var path = Uri.TryCreate(blobPath, UriKind.Absolute, out var uri)
            ? uri.AbsolutePath
            : blobPath;

        var segments = path
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
            return null;

        var containers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            IWorkspaceStorageManagementService.AzureExternalUploadContainerName,
            IWorkspaceStorageManagementService.AzureSharedContainerName,
            IWorkspaceStorageManagementService.AzureExternalUsersContainerName
        };

        var userSegmentIndex = containers.Contains(segments[0])
            ? 1
            : (segments.Length > 2 && containers.Contains(segments[1]) ? 2 : 1);

        if (userSegmentIndex >= segments.Length)
            return null;

        var candidate = segments[userSegmentIndex].Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(candidate))
            return null;

        var atIndex = candidate.IndexOf('_');
        if (atIndex >= 0)
        {
            candidate = string.Concat(candidate.AsSpan(0, atIndex), "@", candidate.AsSpan(atIndex + 1));
        }

        return candidate;
    }

    private static ScanStatusType DetermineScanStatus(string scanError)
    {
        return string.IsNullOrWhiteSpace(scanError)
            ? ScanStatusType.Clean
            : ScanStatusType.Failed;
    }
}
