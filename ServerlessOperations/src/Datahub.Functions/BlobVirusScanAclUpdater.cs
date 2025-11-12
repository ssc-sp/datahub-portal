using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Core.Model.Context;
using Datahub.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Datahub.Functions;

/// <summary>
/// Azure Function that processes blob metadata change events from Event Grid.
/// When a file's virus scan status is set to "Clean", this function updates the ACLs
/// to make the file readable for all workspace members.
/// </summary>
public class BlobVirusScanAclUpdater
{
    private const string SCAN_STATUS_METADATA_KEY = "dh:scanStatus";
    private const string CLEAN_SCAN_STATUS = "Clean";
    
    private readonly ILogger<BlobVirusScanAclUpdater> _logger;
    private readonly DatahubProjectDBContext _dbContext;
    private readonly IBlobAclService _blobAclService;

    public BlobVirusScanAclUpdater(
        ILoggerFactory loggerFactory,
        DatahubProjectDBContext dbContext,
        IBlobAclService blobAclService)
    {
        _logger = loggerFactory.CreateLogger<BlobVirusScanAclUpdater>();
        _dbContext = dbContext;
        _blobAclService = blobAclService;
    }

    /// <summary>
    /// Processes Event Grid events for blob metadata changes.
    /// Triggers when metadata is updated on blobs in Azure Storage.
    /// </summary>
    /// <param name="eventGridEvent">The Event Grid event containing blob metadata change information</param>
    [Function("BlobVirusScanAclUpdater")]
    public async Task Run(
        [EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("Blob metadata change event received. Event Type: {EventType}, Subject: {Subject}", 
            eventGridEvent.EventType, 
            eventGridEvent.Subject);

        try
        {
            // Validate event type
            // Microsoft.Storage.BlobPropertiesUpdated is configured in Terraform (event.tf)
            // This event fires when blob properties or metadata change
            if (eventGridEvent.EventType != "Microsoft.Storage.BlobMetadataUpdated" &&
                eventGridEvent.EventType != "Microsoft.Storage.BlobPropertiesUpdated")
            {
                _logger.LogDebug("Ignoring event type: {EventType}", eventGridEvent.EventType);
                return;
            }

            // Parse event data
            var eventData = eventGridEvent.Data.ToObjectFromJson<BlobEventData>();
            if (eventData == null)
            {
                _logger.LogWarning("Failed to parse event data");
                return;
            }

            var blobUrl = eventData.Url;
            _logger.LogInformation("Processing blob: {BlobUrl}", blobUrl);

            // Extract blob information
            var blobUri = new Uri(blobUrl);
            var blobClient = new BlobClient(blobUri);
            
            // Get blob metadata
            var properties = await blobClient.GetPropertiesAsync();
            var metadata = properties.Value.Metadata;

            // Check if scan status is present and is "Clean"
            if (!metadata.TryGetValue(SCAN_STATUS_METADATA_KEY, out var scanStatus))
            {
                _logger.LogDebug("Blob does not have scan status metadata: {BlobUrl}", blobUrl);
                return;
            }

            if (!string.Equals(scanStatus, CLEAN_SCAN_STATUS, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Blob scan status is '{ScanStatus}', not 'Clean'. Skipping ACL update.", scanStatus);
                return;
            }

            _logger.LogInformation("Blob scan status is 'Clean'. Updating ACLs for workspace members.");

            // Extract workspace/project acronym from blob path
            var workspaceAcronym = ExtractWorkspaceFromBlobPath(blobUri);
            if (string.IsNullOrEmpty(workspaceAcronym))
            {
                _logger.LogWarning("Could not determine workspace acronym from blob path: {BlobUrl}", blobUrl);
                return;
            }

            // Update ACLs to grant read access to all workspace members
            await _blobAclService.GrantWorkspaceMembersReadAccessAsync(blobUri, workspaceAcronym);

            _logger.LogInformation("Successfully updated ACLs for blob: {BlobUrl} in workspace: {Workspace}", 
                blobUrl, workspaceAcronym);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing blob metadata change event: {Subject}", eventGridEvent.Subject);
            throw;
        }
    }

    /// <summary>
    /// Extracts the workspace acronym from the blob path.
    /// Expected format: /blobServices/default/containers/{container}/blobs/{path}
    /// or container name might be the workspace acronym
    /// </summary>
    private string? ExtractWorkspaceFromBlobPath(Uri blobUri)
    {
        try
        {
            var blobClient = new BlobClient(blobUri);
            var containerName = blobClient.BlobContainerName;
            
            // Container name might be the workspace acronym or contain it
            // You may need to adjust this logic based on your naming conventions
            return containerName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract workspace from blob URI: {BlobUri}", blobUri);
            return null;
        }
    }

    /// <summary>
    /// Data structure for blob event data from Event Grid
    /// </summary>
    private class BlobEventData
    {
        public string Api { get; set; } = string.Empty;
        public string ClientRequestId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string ETag { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long ContentLength { get; set; }
        public string BlobType { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Sequencer { get; set; } = string.Empty;
        public Dictionary<string, string>? StorageDiagnostics { get; set; }
    }
}
