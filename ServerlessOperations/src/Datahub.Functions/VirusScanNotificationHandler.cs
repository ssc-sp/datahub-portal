using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Functions.Extensions;
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
using System.Text.RegularExpressions;
using static MudBlazor.FilterOperator;

namespace Datahub.Functions;

public class VirusScanNotificationHandler(
    ILogger<VirusScanNotificationHandler> logger,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    ILockedUserManagementService lockedUserManagementService,
    IGCNotifyService gcNotifyService,
    ISystemTokenCredentialService systemTokenCredentialService,
    IWorkspaceStorageManagementService workspaceStorageManagementService)
{
    [Function("ClamAVNotificationHandler")]
    public async Task RunAsync(
        [ServiceBusTrigger(QueueConstants.ClamAVScanResultQueueName,
            Connection = "DatahubServiceBus:ConnectionString")]
        ServiceBusReceivedMessage message)
    {
        logger.LogInformation("Processing ClamAV scan result message");
        try
        {
            var scanResult = await message.DeserializeAndUnwrapRootAsync<ClamAVMessage>();

            var storageAccountName = ExtractStorageAccountName(scanResult.ScannedFile);
            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                logger.LogWarning("Failed to extract storage account name from scanned file URI {ScannedFile}", scanResult.ScannedFile);
            }

            var workspaceAcronym = !string.IsNullOrWhiteSpace(storageAccountName)
                ? await ResolveWorkspaceAcronymAsync(storageAccountName) ?? storageAccountName
                : "unknown";
            var scanStatus = DetermineScanStatus(scanResult.ScanError);
            var fileName = Path.GetFileName(scanResult.ScannedFile);
            var blobPath = scanResult.ScannedFile;

            // read metadata either from the scan result or from the original blob if not present
            var originalBlobMetadata = await ResolveOriginalBlobMetadataAsync(scanResult, scanResult.ScannedFile);
            var uploader = !string.IsNullOrWhiteSpace(originalBlobMetadata?.CreatedBy)
                ? originalBlobMetadata.CreatedBy
                : ExtractUserFromPath(scanResult.ScannedFile);

            if (scanStatus == ScanStatusType.Clean)
            {
                // No virus detected, move the blob to the user's container and queue the status message
                var targetBlobPath = await workspaceStorageManagementService.MoveBlobToUsersContainerAsync(
                    scanResult.ScannedFile,
                    systemTokenCredentialService.GetTokenCredential());

                if (targetBlobPath != null)
                {
                    await QueueUserStatusAsync(new VirusScanStatusMessage
                    {
                        WorkspaceAcronym = workspaceAcronym,
                        UploaderEmail = uploader,
                        UploadBatchId = originalBlobMetadata?.UploadBatchId ?? System.Guid.NewGuid(),
                        FileId = originalBlobMetadata?.FileId ?? System.Guid.NewGuid(),
                        FileName = fileName,
                        BlobPath = targetBlobPath,
                        ScanStatus = scanStatus,
                        ScanCompletedOn = scanResult.ScanEndTime,
                        FileSizeBytes = null,
                        StorageAccountName = storageAccountName,
                        ScanEngine = "ClamAV"
                    });
                }

                logger.LogInformation(
                    "Forwarded virus scan status for {Workspace}/{FileName} to {QueueName}",
                    workspaceAcronym,
                    fileName,
                    QueueConstants.VirusScanStatusQueueName);
            }
            else if (scanStatus == ScanStatusType.Infected) 
            {
                // notify the default mailbox and the workspace lead about the infected file
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
                await QueueUserStatusAsync(new VirusScanStatusMessage
                {
                    WorkspaceAcronym = workspaceAcronym,
                    UploaderEmail = uploader,
                    UploadBatchId = originalBlobMetadata?.UploadBatchId ?? System.Guid.NewGuid(),
                    FileId = originalBlobMetadata?.FileId ?? System.Guid.NewGuid(),
                    FileName = fileName,
                    BlobPath = scanResult.ScannedFile,
                    ScanStatus = scanStatus,
                    ScanCompletedOn = scanResult.ScanEndTime,
                    FileSizeBytes = null,
                    StorageAccountName = storageAccountName,
                    ScanEngine = "ClamAV"
                });
            } else
            {
                // notify the default mailbox and the workspace lead about the infected file
                var scanCompletedOn = scanResult.ScanEndTime.ToString("g");
                await gcNotifyService.SendBugReportNotification(
                    fileName,
                    $"Unexpected scan status: {scanStatus}",
                    $"ClamAV scan result for {fileName} in workspace {workspaceAcronym} returned an unexpected scan status: {scanStatus}. " +
                    $"Scan completed on {scanCompletedOn}. Scan error: {scanResult.ScanError}");
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

    private static string? ExtractStorageAccountName(string? blobUri)
    {
        if (string.IsNullOrWhiteSpace(blobUri))
        {
            return null;
        }

        if (!Uri.TryCreate(blobUri, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var host = uri.Host;
        var accountName = host.Split('.', 2)[0];
        return string.IsNullOrWhiteSpace(accountName) ? null : accountName;
    }

    private async Task<string?> ResolveWorkspaceAcronymAsync(string storageAccountName)
    {
        try
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var tfType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob)?? throw new InvalidOperationException("Failed to get Terraform service type for AzureStorageBlob");
            var normalizedAccountName = storageAccountName.ToLower();
            var project = await ctx.Project_Resources2.Include(p => p.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.ResourceType == tfType &&
                    r.JsonContent != null &&
                    r.JsonContent.ToLower().Contains(normalizedAccountName));

            return project?.Project?.Project_Acronym_CD;
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

    /// <summary>
    /// Extracts the user identifier from the blob path if the email was not present in the message or metadata
    /// Any file uploaded from the portal will have the metadata "createdby" set to the user email, this is a safety net in case the metadata is missing
    /// </summary>
    /// <param name="blobPath">The path of the blob.</param>
    /// <returns>The extracted user identifier, or null if it cannot be determined.</returns>
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

    private async Task<ClamAVBlobMetadata?> ResolveOriginalBlobMetadataAsync(ClamAVMessage scanResult, string? blobUri)
    {
        if (scanResult.OriginalBlobMetadata is not null)
        {
            return scanResult.OriginalBlobMetadata;
        }

        if (string.IsNullOrWhiteSpace(blobUri))
        {
            return null;
        }

        try
        {
            var metadata = await ReadBlobMetadataAsync(blobUri);
            if (metadata is not null)
            {
                logger.LogInformation("Retrieved blob metadata from source blob for {BlobPath}", blobUri);
                return metadata;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read blob metadata for {BlobPath}", blobUri);
        }

        return null;
    }

    private async Task<ClamAVBlobMetadata?> ReadBlobMetadataAsync(string blobUri)
    {
        if (!Uri.TryCreate(blobUri, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var credential = new DefaultAzureCredential();
        var blobClient = new BlobClient(uri, credential);

        var properties = await blobClient.GetPropertiesAsync();
        var metadata = new ClamAVBlobMetadata();

        if (properties.Value.Metadata.TryGetValue(ClamAVBlobMetadata.CreatedByTag, out var createdBy))
        {
            metadata.CreatedBy = createdBy;
        }
        if (properties.Value.Metadata.TryGetValue(ClamAVBlobMetadata.FileIdTag, out var fileId) && System.Guid.TryParse(fileId, out var guid))
        {
            metadata.FileId = guid;
        }
        if (properties.Value.Metadata.TryGetValue(ClamAVBlobMetadata.UploadBatchIdTag, out var uploadBatch) && System.Guid.TryParse(uploadBatch, out var uploadGuid))
        {
            metadata.UploadBatchId = uploadGuid;
        }

        return metadata;
    }
}
