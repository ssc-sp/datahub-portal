using Azure.Storage.Blobs;
using Datahub.Core.Model.Context;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MassTransit;
using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;
using Datahub.Application.Services.Storage;

namespace Datahub.Functions;

/// <summary>
/// Enriches minimal ClamAV scan result messages with workspace and user context
/// by reading blob metadata and looking up workspace information from storage account name.
/// </summary>
public class ClamAvScanResultEnricher(
    ILogger<ClamAvScanResultEnricher> logger,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IConfiguration configuration)
{
    [Function("ClamAvScanResultEnricher")]
    public async Task RunAsync(
        [QueueTrigger(QueueConstants.ClamAvScanResultQueueName,
            Connection = "DatahubStorageQueue:ConnectionString")]
        string message)
    {
        logger.LogInformation("Processing ClamAV scan result message for enrichment");
        try
        {
            var scanResult = JsonSerializer.Deserialize<ClamAvScanResultMessage>(message);
            if (scanResult == null)
            {
                logger.LogWarning("Failed to deserialize ClamAV scan result message");
                return;
            }

            logger.LogInformation(
                "ClamAV scan result received - File: {ScannedFile}, Error: {ScanError}",
                scanResult.ScannedFile,
                scanResult.ScanError);

            // Extract storage account name from connection string
            var connectionString = configuration.GetConnectionString("DatahubStorageQueue:ConnectionString");
            var storageAccountName = ExtractStorageAccountName(connectionString);

            if (string.IsNullOrEmpty(storageAccountName))
            {
                logger.LogError("Failed to extract storage account name from connection string");
                return;
            }

            // Look up workspace from storage account
            var workspaceAcronym = await ResolveWorkspaceAcronymAsync(storageAccountName);
            if (string.IsNullOrEmpty(workspaceAcronym))
            {
                logger.LogWarning(
                    "Could not resolve workspace for storage account {StorageAccount}. Using storage account name as fallback.",
                    storageAccountName);
                workspaceAcronym = storageAccountName;
            }

            // Read blob metadata to get scan status and user info
            var blobClient = new BlobClient(connectionString, "datahub", scanResult.ScannedFile);
            var properties = await blobClient.GetPropertiesAsync();
            var metadata = properties.Value.Metadata;

            var scanStatus = DetermineScanStatus(scanResult.ScanError, metadata);
            var userObjectId = ExtractUserFromPath(scanResult.ScannedFile);
            if (userObjectId is null && metadata.TryGetValue("uploader_id", out var uploaderId))
            {
                userObjectId = uploaderId;
            }

            // Build enriched message
            var enrichedMessage = new VirusScanNotificationMessage
            {
                WorkspaceAcronym = workspaceAcronym,
                UserObjectId = userObjectId,
                FileName = Path.GetFileName(scanResult.ScannedFile),
                BlobPath = scanResult.ScannedFile,
                ScanStatus = scanStatus,
                ScanCompletedOn = scanResult.ScanEndTime,
                FileSizeBytes = properties.Value.ContentLength,
                StorageAccountName = storageAccountName,
                CorrelationId = Guid.NewGuid().ToString()
            };

            // Send enriched message to downstream queue
            await SendQueueMessageAsync(QueueConstants.VirusScanNotificationQueueName, enrichedMessage);

            logger.LogInformation(
                "Enriched scan result for {Workspace}/{FileName} with status {Status}",
                workspaceAcronym,
                enrichedMessage.FileName,
                scanStatus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enrich ClamAV scan result");
            throw;
        }
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
        // Try to extract user from path patterns like:
        // upload/<user>/filename -> returns <user>
        // shared/<user>/filename -> returns <user>
        var match = Regex.Match(blobPath, @"^(?:upload|shared)/([^/]+)/", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string DetermineScanStatus(string scanError, IDictionary<string, string> metadata)
    {
        // Check for scan execution errors first
        if (!string.IsNullOrEmpty(scanError))
            return "Failed";

        // Check blob metadata for scan result
        if (metadata.TryGetValue("avscan", out var avScanResult))
        {
            return avScanResult.Equals("ok", StringComparison.OrdinalIgnoreCase)
                ? "Clean"
                : "Infected";
        }

        // Default to Failed if no metadata found
        return "Failed";
    }

    private async Task SendQueueMessageAsync(string queueName, object message)
    {
        var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
        await endpoint.Send(message);
    }

    /// <summary>
    /// Minimal message format written by ClamAV container
    /// </summary>
    private record ClamAvScanResultMessage
    {
        public DateTimeOffset ScanStartTime { get; init; }
        public DateTimeOffset ScanEndTime { get; init; }
        public string ScanError { get; init; } = string.Empty;
        public string ScannedFile { get; init; } = string.Empty;
    }
}
