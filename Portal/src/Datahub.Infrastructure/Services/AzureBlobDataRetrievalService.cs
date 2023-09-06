using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using Datahub.Application.Configuration;
using Datahub.Core.Data;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public class AzureBlobDataRetrievalService
{
    private const long MaxFileSize = 10*1024*1024*1024L; // 10GB
    private const string METADATA_FILE_ID = "fileid";
    
    protected readonly ILogger<ProjectDataRetrievalService> _logger;

    public AzureBlobDataRetrievalService(ILogger<ProjectDataRetrievalService> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> GetContainersAsync(string connectionData)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);
        var dfsServiceClient = new DataLakeServiceClient(connectionString.Value);

        // list all of the root folders
        var pages = dfsServiceClient.GetFileSystemsAsync().AsPages();

        var containers = new List<string>();
        await foreach (var page in pages)
        {
            containers.AddRange(page.Values.Select(c => c.Name));
        }

        return containers;
    }

    public async Task<Uri> GenerateSasTokenAsync(string connectionData, string containerName, int days)
    {
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentException($"'{nameof(containerName)}' cannot be null or whitespace.", nameof(containerName));
        }

        var connectionString = await MapConnectionStringAsync(connectionData);
        var containerClient = GetBlobContainerClient(connectionString, containerName);
        var sasBuilder = GetContainerSasBuild(containerName, days, BlobSasPermissions.All);
        var sharedKeyCred = GetSharedKeyCredentialAsync(connectionString);

        var blobUriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
        };

        return blobUriBuilder.ToUri();
    }

    public async Task<(List<string> Folders, List<FileMetaData> Files, string Token)> GetDfsPagesAsync(string connectionData, 
        string containerName, string folderPath, string? continuationToken)
    {
        List<string> folders = new();
        List<FileMetaData> files = new();

        var connectionString = await MapConnectionStringAsync(connectionData);

        var dirClient = GetDirectoryClient(connectionString, containerName, folderPath);
        if (dirClient is null)
            return (folders, files, continuationToken!);

        // iterate the folder
        await IterateDataLakeDirectoryAsync(dirClient, continuationToken, folders.Add, files.Add, ct => continuationToken = ct);

        return (folders, files, continuationToken!);
    }

    public async Task<bool> FileExistsAsync(string connectionData, string containerName, string filePath)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var fs = GetFileSystemClient(connectionString, containerName);
        if (fs is null) 
            return false;

        var fileClient = fs.GetFileClient(filePath);
        if (fileClient is null)
            return false;

        return fileClient.Exists();
    }

    public async Task<Uri> DownloadFileAsync(string connectionData, string containerName, string filePath)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var containerClient = GetBlobContainerClient(connectionString, containerName);
        var sasBuilder = GetBlobSasBuilder(containerName, filePath, 1, BlobSasPermissions.Read);
        var sharedKeyCred = GetSharedKeyCredentialAsync(connectionString);

        var blobClient = containerClient.GetBlobClient(filePath);
        var uri = blobClient.Uri;
        var blobUriBuilder = new BlobUriBuilder(uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
        };
        return blobUriBuilder.ToUri();
    }

    public async Task<bool> UploadFileAsync(string connectionData, string containerName, FileMetaData file, Action<long> progess)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        // get the directory client
        var dirClient = GetDirectoryClient(connectionString, containerName, file.folderpath);
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
            var message = $"File upload in container: '{containerName}', file name: '{file.name}' has failed with error: {ex.Message}";
            _logger.LogError(message, ex);
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string connectionData, string container, string filePath)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var fs = GetFileSystemClient(connectionString, container);
        if (fs is null)
            return false;

        // try to delete the file
        var response = await fs.DeleteFileAsync(filePath);

        // got a response and it is not an error response
        return response is not null && !response.IsError;
    }

    public async Task<bool> RenameFileAsync(string connectionData, string container, string oldFilePath, string newFilePath)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var fs = GetFileSystemClient(connectionString, container);
        if (fs is null)
            return false;

        var fileClient = fs.GetFileClient(oldFilePath);
        if (fileClient is null)
            return false;

        // try to rename the file
        var response = await fileClient.RenameAsync(newFilePath);

        return response is not null;
    }

    public async Task<bool> DeleteFolderAsync(string connectionData, string containerName, string folderPath)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var fs = GetFileSystemClient(connectionString, containerName);
        if (fs is null)
            return false;

        var dirClient = fs.GetDirectoryClient(folderPath);
        if (!await dirClient.ExistsAsync())
            return false;

        // try delete the folder
        var result = await dirClient.DeleteAsync();

        return result?.IsError == false;
    }

    public async Task<bool> CreateFolderAsync(string connectionData, string containerName, string directoryPath, string folderName)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var dirClient = GetDirectoryClient(connectionString, containerName, directoryPath);
        
        if (dirClient is null)
            return false;

        return await dirClient.CreateSubDirectoryAsync(folderName) is not null;
    }

    public async Task<StorageMetadata> GetStorageMetadataAsync(string connectionData, string containerName)
    {
        var connectionString = await MapConnectionStringAsync(connectionData);

        var blobServiceClient = new BlobServiceClient(connectionString.Value);
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

    private BlobContainerClient GetBlobContainerClient(StorageConnectionString connectionString, string containerName)
    {
        var blobServiceClient = new BlobServiceClient(connectionString.Value);
        return blobServiceClient.GetBlobContainerClient(containerName);
    }

    private StorageSharedKeyCredential GetSharedKeyCredentialAsync(StorageConnectionString connectionString)
    {
        var (accountName, accountKey) = connectionString.Parse();
        return new StorageSharedKeyCredential(accountName, accountKey);
    }

    protected virtual Task<StorageConnectionString> MapConnectionStringAsync(string connectionData) => Task.FromResult(new StorageConnectionString(connectionData));

    private DataLakeFileSystemClient? GetFileSystemClient(StorageConnectionString connectionString, string containerName)
    {
        var client = new DataLakeServiceClient(connectionString.Value);
        return client is not null ? client.GetFileSystemClient(containerName) : default;
    }

    private DataLakeDirectoryClient? GetDirectoryClient(StorageConnectionString connectionString, string containerName, string path)
    {
        var fs = GetFileSystemClient(connectionString, containerName);
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

    static BlobSasBuilder GetBlobSasBuilder(string container, string fileName, int days, BlobSasPermissions permissions)
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

    protected static BlobSasBuilder GetContainerSasBuild(string containerName, int days, BlobSasPermissions permissions)
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

    static (string? AccountName, string? AccountKey) ParseConnectionString(string? text)
    {
        string? FindValue(string[] valuePairs, string varName)
        {
            return valuePairs.Where(p => p.StartsWith(varName)).Select(p => p[varName.Length..]).FirstOrDefault();
        }

        var valuePairs = (text ?? "").Split(";", StringSplitOptions.RemoveEmptyEntries);

        return (FindValue(valuePairs, "AccountName="), FindValue(valuePairs, "AccountKey="));
    }
}

public record StorageConnectionString(string Value)
{
    public (string? AccountName, string? AccountKey) Parse()
    {
        var valuePairs = (Value ?? "").Split(";", StringSplitOptions.RemoveEmptyEntries);

        return (FindValue(valuePairs, "AccountName="), FindValue(valuePairs, "AccountKey="));
    }

    static string? FindValue(string[] valuePairs, string varName)
    {
        return valuePairs.Where(p => p.StartsWith(varName)).Select(p => p[varName.Length..]).FirstOrDefault();
    }
}
