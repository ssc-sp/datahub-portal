using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Datahub.Application.Services;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions.Services;

/// <summary>
/// Writes metadata updates back to workspace storage accounts when antivirus scans complete.
/// </summary>
public class BlobMetadataWriter : IBlobMetadataWriter
{
    private readonly IProjectStorageConfigurationService _storageConfig;
    private readonly ILogger<BlobMetadataWriter> _logger;
    private const string ContainerName = "datahub";

    public BlobMetadataWriter(
        IProjectStorageConfigurationService storageConfig,
        ILogger<BlobMetadataWriter> logger)
    {
        _storageConfig = storageConfig;
        _logger = logger;
    }

    public async Task SetAccessEnabledMetadataAsync(
        string workspaceAcronym,
        string blobPath,
        IReadOnlyDictionary<string, string>? metadataSnapshot,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileClient = await GetFileClientAsync(workspaceAcronym, blobPath, cancellationToken);
            var properties = await fileClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            var metadata = new Dictionary<string, string>(properties.Value.Metadata, StringComparer.OrdinalIgnoreCase);

            // Preserve dh metadata from the Event Grid payload if present (primarily dh:scanStatus)
            if (metadataSnapshot is not null)
            {
                foreach (var pair in metadataSnapshot)
                {
                    metadata[pair.Key] = pair.Value;
                }
            }

            metadata["dh:accessEnabledAt"] = DateTimeOffset.UtcNow.ToString("O");

            await fileClient.SetMetadataAsync(metadata, cancellationToken: cancellationToken);
            _logger.LogInformation(
                "Set dh:accessEnabledAt for workspace {Workspace} blob {BlobPath}",
                workspaceAcronym,
                blobPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set access-enabled metadata for workspace {Workspace} blob {BlobPath}",
                workspaceAcronym,
                blobPath);
            throw;
        }
    }

    private async Task<DataLakeFileClient> GetFileClientAsync(
        string workspaceAcronym,
        string blobPath,
        CancellationToken cancellationToken)
    {
        var accountName = _storageConfig.GetProjectStorageAccountName(workspaceAcronym);
        var accountKey = await _storageConfig.GetProjectStorageAccountKey(workspaceAcronym);
        var sharedKey = new StorageSharedKeyCredential(accountName, accountKey);
        var serviceClient = new DataLakeServiceClient(
            new Uri($"https://{accountName}.dfs.core.windows.net"),
            sharedKey);

        var fileSystemClient = serviceClient.GetFileSystemClient(ContainerName);
        var normalizedPath = blobPath.TrimStart('/');
        return fileSystemClient.GetFileClient(normalizedPath);
    }
}
