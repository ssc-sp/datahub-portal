using Azure.Storage;
using Azure.Storage.Blobs;
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

namespace Datahub.Infrastructure.Services;

public class ProjectDataRetrievalService : IProjectDataRetrievalService
{
    private const long MaxFileSize = 10*1024*1024*1024L; // 10GB
    private const string METADATA_FILE_ID = "fileid";

    private readonly ILogger<ProjectDataRetrievalService> _logger;
    private readonly DatahubPortalConfiguration _portalConfiguration;

    public ProjectDataRetrievalService(ILogger<ProjectDataRetrievalService> logger, DatahubPortalConfiguration portalConfiguration)
    {
        _logger = logger;
        _portalConfiguration = portalConfiguration;
    }

    public async Task<List<string>> GetContainersAsync(string projectAcronym)
    {
        var dfsServiceClient = await GetDataLakeServiceClientAsync(projectAcronym);

        // list all of the root folders
        var pages = dfsServiceClient.GetFileSystemsAsync().AsPages();

        var containers = new List<string>();
        await foreach (var page in pages)
        {
            containers.AddRange(page.Values.Select(c => c.Name));
        }

        return containers;
    }

    public async Task<Uri> GenerateSasTokenAsync(string projectAcronym, string containerName, int days)
    {
        var project = projectAcronym.ToLowerInvariant();
        var containerClient = await GetBlobContainerClient(project, containerName);
        var sasBuilder = GetContainerSasBuild(containerName, days, BlobSasPermissions.All);
        var sharedKeyCred = await GetSharedKeyCredentialAsync(project);

        var blobUriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
        };

        return blobUriBuilder.ToUri();
    }

    public async Task<(List<string> Folders, List<FileMetaData> Files, string Token)> GetDfsPagesAsync(string projectAcronym, 
        string containerName, string folderPath, string? continuationToken)
    {
        List<string> folders = new();
        List<FileMetaData> files = new();

        var dirClient = await GetDirectoryClient(projectAcronym, containerName, folderPath);
        if (dirClient is null)
            return (folders, files, continuationToken!);

        // iterate the folder
        await IterateDataLakeDirectoryAsync(dirClient, continuationToken, folders.Add, files.Add, ct => continuationToken = ct);

        return (folders, files, continuationToken!);
    }

    public async Task<bool> FileExistsAsync(string projectAcronym, string containerName, string filePath)
    {
        var fs = await GetFileSystemClient(projectAcronym, containerName);
        if (fs is null) 
            return false;

        var fileClient = fs.GetFileClient(filePath);
        if (fileClient is null)
            return false;

        return fileClient.Exists();
    }

    public async Task<Uri> DownloadFileAsync(string projectAcronym, string containerName, string filePath)
    {
        return await DownloadBlobFile(projectAcronym, containerName, filePath);
    }

    public async Task<bool> UploadFileAsync(string projectAcronym, string containerName, FileMetaData file, Action<long> progess)
    {
        // get the directory client
        var dirClient = await GetDirectoryClient(projectAcronym, containerName, file.folderpath);
        if (dirClient is null)
            return false;

        // create the file
        var fileClient = dirClient.GetFileClient(file.filename);
        if (fileClient is null)
            return false;

        // generate the options with the metadata
        DataLakeFileUploadOptions options = new()
        {
            Metadata = new Dictionary<string, string>()
            {
                { FileMetaData.FileId, file.id },
                { FileMetaData.CreatedBy, file.createdby }
            },
            ProgressHandler = new UploadProgressHandler(progess)
        };

        try
        {
            var result = await fileClient.UploadAsync(file.BrowserFile.OpenReadStream(MaxFileSize), options);
            return result is not null;
        }
        catch (Exception ex)
        {
            var message = $"File upload in project: '{projectAcronym}', container: '{containerName}', file name: '{file.name}' has failed with error: {ex.Message}";
            _logger.LogError(message, ex);
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string projectAcronym, string container, string filePath)
    {
        var fs = await GetFileSystemClient(projectAcronym, container);
        if (fs is null)
            return false;

        // try to delete the file
        var response = await fs.DeleteFileAsync(filePath);

        // got a response and it is not an error response
        return response is not null && !response.IsError;
    }

    public async Task<bool> RenameFileAsync(string projectAcronym, string container, string oldFilePath, string newFilePath)
    {
        var fs = await GetFileSystemClient(projectAcronym, container);
        if (fs is null)
            return false;

        var fileClient = fs.GetFileClient(oldFilePath);
        if (fileClient is null)
            return false;

        // try to rename the file
        var response = await fileClient.RenameAsync(newFilePath);

        return response is not null;
    }

    public async Task<bool> DeleteFolderAsync(string projectAcronym, string containerName, string folderPath)
    {
        var fs = await GetFileSystemClient(projectAcronym, containerName);
        if (fs is null)
            return false;

        var dirClient = fs.GetDirectoryClient(folderPath);
        if (!await dirClient.ExistsAsync())
            return false;

        // try delete the folder
        var result = await dirClient.DeleteAsync();

        return result?.IsError == false;
    }

    public async Task<bool> CreateFolderAsync(string projectAcronym, string containerName, string directoryPath, string folderName)
    {
        var dirClient = await GetDirectoryClient(projectAcronym, containerName, directoryPath);
        
        if (dirClient is null)
            return false;

        return await dirClient.CreateSubDirectoryAsync(folderName) is not null;
    }

    public async Task<StorageMetadata> GetStorageMetadataAsync(string projectAcronym, string containerName)
    {
        var connectionString = await GetProjectConnectionStringAsync(projectAcronym);
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var accountInfo = (await blobServiceClient.GetAccountInfoAsync()).Value;

        StorageMetadata storageMetadata = new()
        {
            Container = containerName,
            Url = containerClient.Uri.ToString(),
            Versioning = "True",
            GeoRedundancy = accountInfo.SkuName.ToString(),
            StorageAccountType = accountInfo.AccountKind.ToString()
        };

        return storageMetadata;
    }

    private static BlobSasBuilder GetContainerSasBuild(string containerName, int days, BlobSasPermissions permissions)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            Resource = "c",
            StartsOn = DateTimeOffset.Now,
            ExpiresOn = DateTimeOffset.Now.AddDays(days)
        };

        sasBuilder.SetPermissions(permissions);

        return sasBuilder;
    }

    private async Task<DataLakeFileSystemClient?> GetFileSystemClient(string projectAcronym, string containerName)
    {
        var client = await GetDataLakeServiceClientAsync(projectAcronym);
        return client is not null ? client.GetFileSystemClient(containerName) : default;
    }

    private async Task<DataLakeDirectoryClient?> GetDirectoryClient(string projectAcronym, string containerName, string path)
    {
        var fs = await GetFileSystemClient(projectAcronym, containerName);
        return fs is not null ? fs.GetDirectoryClient(path) : default;
    }

    private async Task IterateDataLakeDirectoryAsync(DataLakeDirectoryClient client, string? continuationToken,
        Action<string> addFolder, Action<FileMetaData> addFile, Action<string?> setContinuationToken)
    {
        await foreach (var page in client.GetPathsAsync().AsPages(continuationToken))
        {
            if (page is null)
                continue;

            setContinuationToken(page.ContinuationToken);
            foreach (var path in page.Values)
            {
                if (path.IsDirectory == true)
                {
                    addFolder(path.Name);
                }
                else
                {
                    var fileMetadata = await GetFileMetadataAsync(client, Path.GetFileName(path.Name));
                    if (fileMetadata is not null)
                    {
                        addFile(fileMetadata);
                    }
                }
            }
        }
    }

    private async Task<FileMetaData?> GetFileMetadataAsync(DataLakeDirectoryClient client, string fileName)
    {
        var fileClient = client.GetFileClient(fileName);
        if (fileClient is null)
            return default;

        var propResponse = await fileClient.GetPropertiesAsync();
        if (propResponse is null)
            return default;

        var props = propResponse.Value;
        var metadata = props.Metadata;

        return new()
        {
            id = GetMetadata(metadata, METADATA_FILE_ID, Guid.NewGuid().ToString()),
            name = fileName,
            ownedby = GetMetadata(metadata, FileMetaData.OwnedBy),
            createdby = GetMetadata(metadata, FileMetaData.CreatedBy),
            lastmodifiedby = GetMetadata(metadata, FileMetaData.LastModifiedBy),
            lastmodifiedts = props.LastModified.DateTime,
            filesize = props.ContentLength.ToString()
        };
    }

    static string? GetMetadata(IDictionary<string, string> dict, string key, string? defaultValue = default)
    {
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }

    private async Task<DataLakeServiceClient> GetDataLakeServiceClientAsync(string projectAcronym)
    {
        var sharedKeyCredential = await GetSharedKeyCredentialAsync(projectAcronym);
        var storageAccountName = GetProjectStorageAccountName(projectAcronym);
        var dfsUri = BuildDfsUriFromStorageAccountName(storageAccountName);
        return new DataLakeServiceClient(new Uri(dfsUri), sharedKeyCredential); // new DefaultAzureCredential());
    }

    private string BuildDfsUriFromStorageAccountName(string accountName)
    {
        return $"https://{accountName}.dfs.core.windows.net";
    }

    private async Task<Uri> DownloadBlobFile(string projectAcronym, string container, string filePath, int daysValidity = 1)
    {
        return await GetDelegationSasBlobUri(projectAcronym, container, filePath, daysValidity, BlobSasPermissions.Read | BlobSasPermissions.Write);
    }

    private async Task<Uri> GetDelegationSasBlobUri(string projectAcronym, string container, string fileName, int days, BlobSasPermissions permissions)
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
        return @$"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={accountKey.Value};EndpointSuffix=core.windows.net";
    }
    
    private string GetProjectStorageAccountName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}proj{projectAcronym.ToLower()}{envName}";
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
        var envName = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev").ToLower();

        // map developemnt or sandbox to dev
        if (envName.Equals("development") || envName.Equals("sand"))
            return "dev";

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

        return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
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
        var projectAcronymLower = projectAcronym.ToLower();
        var storageAccountKey = await GetProjectStorageAccountKeyAsync(projectAcronymLower);
        var storageAccountName = GetProjectStorageAccountName(projectAcronymLower);
        return new StorageSharedKeyCredential(storageAccountName, storageAccountKey.Value);
    }
}

class UploadProgressHandler : IProgress<long>
{
    private readonly Action<long> _onProgress;

    public UploadProgressHandler(Action<long> onProgress)
    {
        _onProgress = onProgress;
    }

    public void Report(long value) => _onProgress.Invoke(value);
}