using System.Globalization;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Data;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Datahub.Infrastructure.Services;

public class ProjectDataRetrievalService : IProjectDataRetrievalService
{
    private const string METADATA_FILE_ID = "fileid";

    private readonly ILogger<ProjectDataRetrievalService> _logger;
    private readonly DatahubPortalConfiguration _portalConfiguration;

    public ProjectDataRetrievalService(ILogger<ProjectDataRetrievalService> logger,
        DatahubPortalConfiguration portalConfiguration)
    {
        _logger = logger;
        _portalConfiguration = portalConfiguration;
    }


    public async Task<(List<string>, List<FileMetaData>, string)> GetStorageBlobPagesAsync(string projectAcronym,
        string containerName, User user, string prefix,
        string? continuationToken = default)
    {
        try
        {
            var folders = new List<string>();
            var files = new List<FileMetaData>();

            var connectionString = await GetProjectConnectionStringAsync(projectAcronym.ToLower());
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var resultSegment = containerClient
                .GetBlobsByHierarchyAsync(prefix: prefix.TrimStart('/'), traits: BlobTraits.Metadata, delimiter: "/")
                .AsPages(continuationToken);

            // Enumerate the blobs returned for each page.
            await foreach (var blobPage in resultSegment)
            {
                continuationToken = blobPage.ContinuationToken;
                foreach (var blobHierarchyItem in blobPage.Values)
                {
                    if (blobHierarchyItem.IsPrefix)
                    {
                        folders.Add(blobHierarchyItem.Prefix);
                    }
                    else
                    {
                        var fileId = await VerifyFileIdMetadata(blobHierarchyItem, containerClient);
                        var fileMetaData = FileMetadataFromBlobItem(blobHierarchyItem, fileId);
                        files.Add(fileMetaData);
                    }
                }

                return (folders, files, continuationToken);
            }

            return (folders, files, continuationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get file list for project: {ProjectAcronym} for user: {DisplayName} FAILED",
                projectAcronym, user.DisplayName);
            throw;
        }
    }

    public async Task<bool> StorageBlobExistsAsync(string filename, string projectAcronym, string containerName)
    {
        var connectionString = await GetProjectConnectionStringAsync(projectAcronym.ToLower());
        var blobServiceClient = new BlobServiceClient(connectionString);
            
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(filename);

        return await blobClient.ExistsAsync();
    }

    public async Task<Uri> DownloadFile(string container, FileMetaData file, string projectAcronym)
    {
        return await DownloadBlobFile(container, file, projectAcronym);
    }

    public async Task<List<string>> GetProjectContainersAsync(string projectAcronym)
    {
        var connectionString = await GetProjectConnectionStringAsync(projectAcronym);
        var blobServiceClient = new BlobServiceClient(connectionString);
        var pages = blobServiceClient.GetBlobContainersAsync().AsPages();
        var containers = new List<string>();
        await foreach (var page in pages)
        {
            containers.AddRange(page.Values.Select(c => c.Name));
        }

        return containers;
    }

    private async Task<Uri> DownloadBlobFile(string container, FileMetaData file, string projectAcronym,
        int daysValidity = 1)
    {
        return await GetDelegationSasBlobUri(container, file.filename, projectAcronym, daysValidity,
            BlobSasPermissions.Read | BlobSasPermissions.Write);
    }

    private async Task<Uri> GetDelegationSasBlobUri(string container, string fileName, string projectAcronym,
        int days, BlobSasPermissions permissions)
    {
        var project = projectAcronym.ToLowerInvariant();
        var containerClient = await GetBlobContainerClient(project, container);
        var sasBuilder = GetBlobSasBuilder(container, fileName, days, permissions);
        var sharedKeyCred = await GetSharedKeyCredentialAsync(project);

        var blobClient = containerClient.GetBlobClient(fileName);
        var uri = blobClient.Uri;
        var blobUriBuilder = new BlobUriBuilder(uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
        };
        return blobUriBuilder.ToUri();
    }


    private async Task<string> GetProjectConnectionStringAsync(string projectAcronym)
    {
        var accountKey = await GetProjectStorageAccountKeyAsync(projectAcronym);
        var storageAccountName = GetProjectStorageAccountName(projectAcronym);

        return
            @$"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
    }
    
    private string GetProjectStorageAccountName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}proj{projectAcronym}{envName}";
    }

    private async Task<SecretBundle> GetProjectStorageAccountKeyAsync(string projectAcronym)
    {
        var key = GetProjectStorageKeyName(projectAcronym);
        var keyVaultName = GetProjectKeyVaultName(projectAcronym);
        var keyVaultClient = GetKeyVaultClient();
        var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net";
        return await keyVaultClient.GetSecretAsync(keyVaultUrl, key);
    }

    private string GetProjectStorageKeyName(string projectAcronym)
    {
        if (_portalConfiguration.CentralizedProjectSecrets)
        {
            return $"datahub-blob-key-{projectAcronym.ToLower()}";
        }

        return _portalConfiguration.ProjectStorageKeySecretName;
    }
    private string GetProjectKeyVaultName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}-proj-{projectAcronym}-{envName}-kv".ToLower();
    }

    private static string GetEnvironmentName()
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        envName = envName != null ? envName.ToLower() : "dev";
        if (envName.Equals("development") || envName.Equals("sand"))
        {
            envName = "dev";
        }

        return envName;
    }

    private async Task<BlobContainerClient> GetBlobContainerClient(string project, string containerName)
    {
        var connectionString = await GetProjectConnectionStringAsync(project);
        var blobServiceClient = new BlobServiceClient(connectionString);
        return blobServiceClient.GetBlobContainerClient(containerName);
    }

    private KeyVaultClient GetKeyVaultClient()
    {
        AzureServiceTokenProvider azureServiceTokenProvider;
        if (_portalConfiguration.PortalRunAsManagedIdentity.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogInformation("Building key vault with Managed Identity");
            azureServiceTokenProvider = new AzureServiceTokenProvider("RunAs=App");
        }
        else
        {
            _logger.LogInformation("Building key vault with Service Principal");
            var tenantId = _portalConfiguration.AzureAd.TenantId;
            var clientId = _portalConfiguration.AzureAd.ClientId;
            var clientSecret = _portalConfiguration.AzureAd.ClientSecret;

            azureServiceTokenProvider =
                new AzureServiceTokenProvider($"RunAs=App;AppId={clientId};TenantId={tenantId};AppKey={clientSecret}");
        }

        return new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
    }

    private static async Task<string> VerifyFileIdMetadata(BlobHierarchyItem blobItem,
        BlobContainerClient containerClient)
    {
        return await VerifyFileIdMetadata(blobItem.Blob, containerClient);
    }

    private static async Task<string> VerifyFileIdMetadata(BlobItem blobItem, BlobContainerClient containerClient)
    {
        if (blobItem.Metadata.TryGetValue(METADATA_FILE_ID, out var fileId))
            return fileId;

        var newId = Guid.NewGuid().ToString();
        blobItem.Metadata.Add(METADATA_FILE_ID, newId);
        var client = containerClient.GetBlobClient(blobItem.Name);
        await client.SetMetadataAsync(blobItem.Metadata);
        fileId = newId;

        return fileId;
    }

    private static FileMetaData FileMetadataFromBlobItem(BlobHierarchyItem blobItem, string fileId)
    {
        return FileMetadataFromBlobItem(blobItem.Blob, fileId);
    }

    private static FileMetaData FileMetadataFromBlobItem(BlobItem blobItem, string fileId)
    {
        string? ownedBy = blobItem.Metadata.TryGetValue(FileMetaData.OwnedBy, out ownedBy) ? ownedBy : "Unknown";
        string? createdBy = blobItem.Metadata.TryGetValue(FileMetaData.CreatedBy, out createdBy)
            ? createdBy
            : "Unknown";
        string? lastModifiedBy = blobItem.Metadata.TryGetValue(FileMetaData.LastModifiedBy, out lastModifiedBy)
            ? lastModifiedBy
            : "lastmodifiedby";

        if (Environment.GetEnvironmentVariable("HostingProfile") == "ssc")
        {
            return new FileMetaData()
            {
                id = fileId,
                filename = blobItem.Name,
                ownedby = ownedBy,
                createdby = createdBy,
                lastmodifiedby = lastModifiedBy,
                lastmodifiedts = blobItem.Properties.LastModified?.DateTime ?? DateTime.Now,
                filesize = blobItem.Properties.ContentLength.ToString()
            };
        }

        string? lastModified = blobItem.Metadata.TryGetValue(FileMetaData.LastModified, out lastModified)
            ? lastModified
            : DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        string? fileSize = blobItem.Metadata.TryGetValue(FileMetaData.FileSize, out fileSize) ? fileSize : "0";

        var isDateValid = DateTime.TryParse(lastModified, out var parsedModifiedDate);
        if (!isDateValid)
            parsedModifiedDate = DateTime.UtcNow;

        return new FileMetaData()
        {
            id = fileId,
            filename = blobItem.Name,
            ownedby = ownedBy,
            createdby = createdBy,
            lastmodifiedby = lastModifiedBy,
            lastmodifiedts = parsedModifiedDate,
            filesize = fileSize
        };
    }

    private static BlobSasBuilder GetBlobSasBuilder(string container, string fileName, int days,
        BlobSasPermissions permissions)
    {
        var result = new BlobSasBuilder()
        {
            BlobContainerName = container,
            BlobName = fileName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(days)
        };

        result.SetPermissions(permissions);

        return result;
    }

    private async Task<StorageSharedKeyCredential> GetSharedKeyCredentialAsync(string projectAcronym)
    {
        var storageAccountKey = await GetProjectStorageAccountKeyAsync(projectAcronym);
        var storageAccountName = GetProjectStorageAccountName(projectAcronym);
        return new StorageSharedKeyCredential(storageAccountName, storageAccountKey.ToString());
    }
}