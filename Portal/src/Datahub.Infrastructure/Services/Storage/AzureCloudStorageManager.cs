using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using Datahub.Core.Data;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Security;
using Datahub.Portal.Pages.Workspace.Storage.ResourcePages;

namespace Datahub.Infrastructure.Services.Storage;

public class AzureCloudStorageManager : ICloudStorageManager
{
    private readonly string _accountName;
    private readonly string _accountKey;
    private readonly bool _inboxAccount;
    private readonly string _connectionString;
    private readonly string _displayName;

    public bool IsInboxAccount => _inboxAccount;

    public AzureCloudStorageManager(string accountName, string accountKey, string? displayName = default)
    {
        _accountName = accountName;
        _accountKey = accountKey;
        _inboxAccount = displayName == default;
        _displayName = displayName ?? _accountName;
        _connectionString = @$"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
    }

    public async Task<List<string>> GetContainersAsync()
    {
        var dlClient = new DataLakeServiceClient(_connectionString);

        var pages = dlClient.GetFileSystemsAsync().AsPages();

        var containers = new List<string>();
        await foreach (var page in pages)
        {
            containers.AddRange(page.Values.Select(c => c.Name));
        }

        return containers;
    }

    public async Task<DfsPage> GetDfsPagesAsync(string container, string folderPath, string? continuationToken = null)
    {
        ValidateContainerName(container);

        List<string> folders = new();
        List<FileMetaData> files = new();

        var dirClient = GetDirectoryClient(container, folderPath);

        // iterate the folder
        await IterateDataLakeDirectoryAsync(dirClient, continuationToken, folders.Add, files.Add, ct => continuationToken = ct);

        return new DfsPage(folders, files, continuationToken!);
    }

    public Task<Uri> GenerateSasTokenAsync(string container, int days)
    {
        ValidateContainerName(container);

        var containerClient = GetBlobContainerClient(container);
        var sasBuilder = GetContainerSasBuild(container, days, BlobSasPermissions.All);
        var sharedKeyCred = GetSharedKeyCredentialAsync();

        var blobUriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
        };

        return Task.FromResult(blobUriBuilder.ToUri());
    }

    public Task<bool> FileExistsAsync(string container, string filePath)
    {
        var fs = GetFileSystemClient(container);
        var fileClient = fs.GetFileClient(filePath);
        return Task.FromResult<bool>(fileClient.Exists());
    }

    public Task<Uri> DownloadFileAsync(string container, string filePath)
    {
        var containerClient = GetBlobContainerClient(container);
        var sasBuilder = GetBlobSasBuilder(container, filePath, 1, BlobSasPermissions.Read);
        var sharedKeyCred = GetSharedKeyCredentialAsync();

        var blobClient = containerClient.GetBlobClient(filePath);
        var uri = blobClient.Uri;
        var blobUriBuilder = new BlobUriBuilder(uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
        };

        return Task.FromResult(blobUriBuilder.ToUri());
    }

    public async Task<bool> UploadFileAsync(string container, FileMetaData file, Action<long> progess)
    {
        // get the directory client
        var dirClient = GetDirectoryClient(container, file.folderpath);

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

        var result = await fileClient.UploadAsync(file.BrowserFile.OpenReadStream(MaxFileSize), options);

        return result is not null;
    }

    public async Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string directoryPath)
    {
        var dirClient = GetDirectoryClient(container, directoryPath);
        return await dirClient.CreateSubDirectoryAsync(directoryPath) is not null;
    }

    public async Task<bool> DeleteFileAsync(string container, string filePath)
    {
        var fs = GetFileSystemClient(container);
        if (fs is null)
            return false;

        // try to delete the file
        var response = await fs.DeleteFileAsync(filePath);

        // got a response and it is not an error response
        return response is not null && !response.IsError;
    }

    public async Task<bool> DeleteFolderAsync(string container, string folderPath)
    {
        var fs = GetFileSystemClient(container);
        if (fs is null)
            return false;

        var dirClient = fs.GetDirectoryClient(folderPath);
        if (!await dirClient.ExistsAsync())
            return false;

        // try delete the folder
        var result = await dirClient.DeleteAsync();

        return result?.IsError == false;
    }

    public async Task<StorageMetadata> GetStorageMetadataAsync(string container)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var accountInfo = (await blobServiceClient.GetAccountInfoAsync()).Value;

        StorageMetadata storageMetadata = new()
        {
            Container = container,
            Url = containerClient.Uri.ToString(),
            Versioning = "True",
            GeoRedundancy = accountInfo.SkuName.ToString(),
            StorageAccountType = accountInfo.AccountKind.ToString()
        };

        return storageMetadata;
    }

    public async Task<bool> RenameFileAsync(string container, string oldFilePath, string newFilePath)
    {
        var fs = GetFileSystemClient(container);
        if (fs is null)
            return false;

        var fileClient = fs.GetFileClient(oldFilePath);
        if (fileClient is null)
            return false;

        // try to rename the file
        var response = await fileClient.RenameAsync(newFilePath);

        return response is not null;
    }

    public bool AzCopyEnabled => true;
    public bool DatabrickEnabled => true;

    public CloudStorageProviderType ProviderType => CloudStorageProviderType.Azure;

    public string DisplayName => _displayName;

    static void ValidateContainerName(string container)
    {
        if (string.IsNullOrWhiteSpace(container))
        {
            throw new ArgumentException($"'{nameof(container)}' cannot be null or whitespace.", nameof(container));
        }
    }

    private DataLakeDirectoryClient GetDirectoryClient(string containerName, string path)
    {
        var fs = GetFileSystemClient(containerName);
        return fs.GetDirectoryClient(path);
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

    private const long MaxFileSize = 10 * 1024 * 1024 * 1024L; // 10GB
    private const string METADATA_FILE_ID = "fileid";

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
            filesize = props.ContentLength.ToString(),
            folderpath = client.Path
        };
    }

    static string? GetMetadata(IDictionary<string, string> dict, string key, string? defaultValue = default)
    {
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }

    private DataLakeFileSystemClient GetFileSystemClient(string containerName)
    {
        var client = new DataLakeServiceClient(_connectionString);
        return client.GetFileSystemClient(containerName);
    }

    private BlobContainerClient GetBlobContainerClient(string containerName)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        return blobServiceClient.GetBlobContainerClient(containerName);
    }

    static BlobSasBuilder GetContainerSasBuild(string containerName, int days, BlobSasPermissions permissions)
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

    private StorageSharedKeyCredential GetSharedKeyCredentialAsync()
    {
        return new StorageSharedKeyCredential(_accountName, _accountKey);
    }

    static BlobSasBuilder GetBlobSasBuilder(string container, string fileName, int days, BlobSasPermissions permissions)
    {
        var result = new BlobSasBuilder()
        {
            BlobContainerName = container,
            BlobName = fileName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddDays(-1),
            ExpiresOn = DateTimeOffset.UtcNow.AddDays(days)
        };

        result.SetPermissions(permissions);

        return result;
    }

    public List<(string Placeholder, string Replacement)> GetSubstitutions(string projectAcronym, CloudStorageContainer container)
    {
        if (_inboxAccount)
        {
            return new List<(string, string)>
            {
                (ResourceSubstitutions.ProjectAcronym, projectAcronym),
                (ResourceSubstitutions.StorageAccount, ResourceSubstitutions.GetStorageAccountNameFromProjectAcronym(projectAcronym)),
                (ResourceSubstitutions.ContainerName, container.Name)
            };
        }
        else
        {

            return new List<(string, string)>
            {
                (ResourceSubstitutions.ProjectAcronym, projectAcronym),
                (ResourceSubstitutions.AZAccountKey, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AZ_AccountKey)),
                (ResourceSubstitutions.AZAccountName, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AZ_AccountName)),
                (ResourceSubstitutions.ContainerName, container.Name)
            };
        }
    }
}
