using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using DefaultAzureCredential = Azure.Identity.DefaultAzureCredential;
using DefaultAzureCredentialOptions = Azure.Identity.DefaultAzureCredentialOptions;
using Datahub.Core.Data;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Security;
using Datahub.Portal.Pages.Workspace.Storage.ResourcePages;
using Microsoft.VisualStudio.Services.Common;
using Datahub.Infrastructure.Services.Helpers;
using Azure.Storage.Blobs.Models;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using System.Security.Claims;
using Azure.Core;

namespace Datahub.Infrastructure.Services.Storage;

public class AzureCloudStorageManager : ICloudStorageManager
{
    public const string ControllerRoute = "download-azfile";

    private readonly string _accountName;
    private readonly TokenCredential? _tokenCredential;
    private readonly string _accountKey;
    private readonly bool _inboxAccount;
    private readonly string _connectionString;
    private readonly Uri _blobServiceUri;
    private readonly string _displayName;
    private readonly string? _sasToken;

    private static readonly TimeSpan DefaultTokenExpiry = TimeSpan.FromHours(1);

    public bool IsInboxAccount => _inboxAccount;

    public AzureCloudStorageManager(string accountName, string accountKey, string? displayName = default)
    {
        _accountName = accountName;
        _accountKey = accountKey;
        _inboxAccount = displayName == default;
        _displayName = displayName ?? _accountName;
        _connectionString = @$"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
        _blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
    }

    /// <summary>
    /// Constructor for user authentication
    /// </summary>
    public AzureCloudStorageManager(string accountName, TokenCredential tokenCredential, string? displayName = default)
    {
        _accountName = accountName;
        _tokenCredential = tokenCredential;
        _accountKey = string.Empty;
        _inboxAccount = displayName == default;
        _displayName = displayName ?? _accountName;
        _blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
        _connectionString = string.Empty;
    }

    public async Task<List<string>> GetContainersAsync()
    {

        var dlClient = _tokenCredential is null
            ? new DataLakeServiceClient(_connectionString)
            : new DataLakeServiceClient(_blobServiceUri, _tokenCredential);

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

    public async Task<Uri> GenerateSasTokenAsync(string container, TimeSpan timeSpan)
    {
        ValidateContainerName(container);

        var containerClient = await GetBlobContainerClient(container);
        if (!string.IsNullOrWhiteSpace(_sasToken))
        {
            return AppendSasToken(containerClient.Uri);
        }

        return await GenerateContainerSasUriAsync(containerClient, timeSpan);
    }

    public Task<bool> FileExistsAsync(string container, string filePath)
    {
        var fs = GetFileSystemClient(container);
        var fileClient = fs.GetFileClient(filePath);
        return Task.FromResult<bool>(fileClient.Exists());
    }

    public async Task<Uri> DownloadFileAsync(string container, string filePath, string userName, IFileTokenService? fileTokenService = null)
    {
        if (fileTokenService is not null)
        {
            var token = fileTokenService.CreateToken(this, _accountName, container, filePath, userName, DefaultTokenExpiry);
            return new Uri($"/{ControllerRoute}?token={token}", UriKind.Relative);
        }

        var containerClient = await GetBlobContainerClient(container);

        var blobClient = containerClient.GetBlobClient(filePath);
        if (!string.IsNullOrWhiteSpace(_sasToken))
        {
            return AppendSasToken(blobClient.Uri);
        }

        var sasBuilder = GetBlobSasBuilder(container, filePath, 1, BlobSasPermissions.Read);
        var sasQueryParameters = await GetSasQueryParametersAsync(sasBuilder);
        var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
        {
            Sas = sasQueryParameters
        };

        return blobUriBuilder.ToUri();
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
                { FileMetaData.CreatedBy, file.createdby },
                { FileMetaData.UploadBatchId, file.uploadBatchId },
            },
            ProgressHandler = new UploadProgressHandler(progess)
        };

        var result = await fileClient.UploadAsync(file.BrowserFile.OpenReadStream(MaxFileSize), options);

        return result is not null;
    }

    public async Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string directoryPath)
    {
        var dirClient = GetDirectoryClient(container, currentWorkingDirectory);
        var createResult = await dirClient.CreateSubDirectoryAsync(directoryPath);
        return createResult is not null;
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
        BlobServiceClient blobServiceClient;
        if (_tokenCredential is null)
        {
            blobServiceClient = new BlobServiceClient(_connectionString);
        }
        else
        {
            blobServiceClient = new BlobServiceClient(_blobServiceUri, _tokenCredential);
        }

        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var accountInfo = (await blobServiceClient.GetAccountInfoAsync()).Value;

        AzureStorageMetadata storageMetadata = new()
        {
            Container = container,
            Url = containerClient.Uri.ToString(),
            Versioning = "True",
            GeoRedundancy = accountInfo.SkuName.ToString(),
            StorageAccountType = accountInfo.AccountKind.ToString(),            
        };

        return storageMetadata;
    }

    /// <summary>
    /// Collects list of folders with number of files in each folder
    /// </summary>
    /// <param name="container"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, int>> ListFoldersAsync(string container, string prefix = "")
    {
        ValidateContainerName(container);

        var containerClient = await GetBlobContainerClient(container);
        var result = new Dictionary<string, int>();

        await TraverseFolderTreeAsync(containerClient, prefix, result);

        return result;
    }

    private async Task TraverseFolderTreeAsync(BlobContainerClient containerClient, string prefix, Dictionary<string, int> result)
    {
        var blobs = containerClient.GetBlobsByHierarchyAsync(new GetBlobsByHierarchyOptions { Prefix = prefix, Traits = BlobTraits.Metadata, Delimiter = "/" });

        int fileCount = 0;
        var subFolders = new List<string>();

        await foreach (var blobHierarchyItem in blobs)
        {
            if (blobHierarchyItem.IsPrefix)
            {
                // It's a folder, add to subFolders list
                subFolders.Add(blobHierarchyItem.Prefix);
            }
            else
            {
                // It's a file, count it
                fileCount++;
            }
        }

        // Add the current folder to the result dictionary
        result[prefix] = fileCount;

        // Traverse subfolders
        foreach (var subFolder in subFolders)
        {
            await TraverseFolderTreeAsync(containerClient, subFolder, result);
        }
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
        var fileMetadataTasks = new List<Task<FileMetaData?>>();

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
                    fileMetadataTasks.Add(GetFileMetadataAsync(client, Path.GetFileName(path.Name)));
                }
            }
        }

        var completedFileMetadata = await Task.WhenAll(fileMetadataTasks);
        completedFileMetadata.Where(f => f is not null).ForEach(f => addFile(f!));
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
            id = GetMetadata(metadata, METADATA_FILE_ID, Guid.NewGuid().ToString())?? throw new InvalidOperationException("File ID is missing"),
            name = fileName,
            ownedby = GetMetadata(metadata, FileMetaData.OwnedBy),
            createdby = GetMetadata(metadata, FileMetaData.CreatedBy),
            lastmodifiedby = GetMetadata(metadata, FileMetaData.LastModifiedBy),
            lastmodifiedts = props.LastModified.DateTime,
            uploadBatchId = GetMetadata(metadata, FileMetaData.UploadBatchId),
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
        if (_tokenCredential is null)
        {
            var client = new DataLakeServiceClient(_connectionString);
            return client.GetFileSystemClient(containerName);
        }
        else
        {
            var client = new DataLakeServiceClient(_blobServiceUri, _tokenCredential);
            return client.GetFileSystemClient(containerName);
        }
    }

    public async Task<BlobContainerClient> GetBlobContainerClient(string containerName)
    {
        BlobServiceClient blobServiceClient;
        if (_tokenCredential is null)
        {
            blobServiceClient = new BlobServiceClient(_connectionString);
        }
        else
        {
            blobServiceClient = new BlobServiceClient(_blobServiceUri, _tokenCredential);
        }    

        return blobServiceClient.GetBlobContainerClient(containerName);
    }

    static BlobSasBuilder GetContainerSasBuild(string containerName, TimeSpan timeSpan, BlobSasPermissions permissions)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            Resource = "c",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.Add(timeSpan)
        };

        sasBuilder.SetPermissions(permissions);

        return sasBuilder;
    }

    private StorageSharedKeyCredential GetSharedKeyCredentialAsync()
    {
        return new StorageSharedKeyCredential(_accountName, _accountKey);
    }

    private async Task<Uri> GenerateContainerSasUriAsync(BlobContainerClient containerClient, TimeSpan timeSpan)
    {
        var sasBuilder = GetContainerSasBuild(containerClient.Name, timeSpan, BlobSasPermissions.All);
        var sasQueryParameters = await GetSasQueryParametersAsync(sasBuilder);

        var blobUriBuilder = new BlobUriBuilder(containerClient.Uri)
        {
            Sas = sasQueryParameters
        };

        return blobUriBuilder.ToUri();
    }

    private async Task<BlobSasQueryParameters> GetSasQueryParametersAsync(BlobSasBuilder sasBuilder)
    {
        var sharedKeyCred = GetSharedKeyCredentialAsync();
        return sasBuilder.ToSasQueryParameters(sharedKeyCred);
    }

    private Uri AppendSasToken(Uri resourceUri)
    {
        var sasToken = _sasToken ?? string.Empty;
        var trimmedToken = sasToken.StartsWith("?") ? sasToken.Substring(1) : sasToken;
        var separator = string.IsNullOrWhiteSpace(resourceUri.Query) ? "?" : "&";
        return new Uri($"{resourceUri}{separator}{trimmedToken}");
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

    public async Task<List<FileMetaData>> SearchFilesAsync(string container, string folderPath, string searchTerm, CancellationToken cancellationToken, bool searchInContent = false)
    {
        ValidateContainerName(container);

        var dirClient = GetDirectoryClient(container, folderPath);
        var matchingFiles = new List<FileMetaData>();

        // Recursively iterate through all paths (files and folders) in the directory
        await foreach (var path in dirClient.GetPathsAsync(recursive: true).WithCancellation(cancellationToken))
        {
            // Skip directories
            if (path.IsDirectory.HasValue && path.IsDirectory.Value)
                continue;

            // Extract the file name from the full path
            var fileName = Path.GetFileName(path.Name);

            // Construct the full path of the file
            string fullPath = path.Name;

            // Retrieve metadata for the file
            var fileMetadata = await GetFileMetadataAsync(dirClient, fullPath);

            if (fileMetadata == null)
                continue;

            // Check if the file name contains the search term
            if (fileName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                matchingFiles.Add(fileMetadata);
                continue;
            }

            // If content search is enabled, check the file content
            if (searchInContent && await FileContentContainsTermAsync(container, fullPath, searchTerm, cancellationToken))
            {
                matchingFiles.Add(fileMetadata);
            }
        }

        return matchingFiles;
    }

    private async Task<bool> FileContentContainsTermAsync(string container, string filePath, string searchTerm, CancellationToken cancellationToken)
    {
        var blobClient = (await GetBlobContainerClient(container)).GetBlobClient(filePath);

        // Download the file content
        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        // Determine file type and search content
        if (filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(memoryStream);
            var content = await reader.ReadToEndAsync();
            return content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        }
        else if (filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            return SearchFileContentHelper.SearchWordDocument(memoryStream, searchTerm);
        }
        else if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return SearchFileContentHelper.SearchPdfDocument(memoryStream, searchTerm);
        }

        return false;
    }

}
