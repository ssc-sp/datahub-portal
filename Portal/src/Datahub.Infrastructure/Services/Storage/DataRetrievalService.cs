using System.Globalization;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using Datahub.Application.Configuration;
using Datahub.Core.Data;
using Datahub.Core.Services.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Folder = Datahub.Core.Data.Folder;
using Version = Datahub.Core.Data.Version;

namespace Datahub.Infrastructure.Services.Storage;

public class DataRetrievalService : BaseService
{
    private const string METADATA_FILE_ID = "fileid";
    public const string DEFAULT_CONTAINER_NAME = "datahub";

    private readonly ILogger<DataRetrievalService> _logger;
    private readonly DataLakeClientService _dataLakeClientService;
    private readonly DatahubPortalConfiguration _portalConfiguration;


    public DataRetrievalService(ILogger<DataRetrievalService> logger,
        DataLakeClientService dataLakeClientService,
        DatahubPortalConfiguration portalConfiguration,
        NavigationManager navigationManager)
        : base(navigationManager)
    {
        _logger = logger;
        _dataLakeClientService = dataLakeClientService;
        _portalConfiguration = portalConfiguration;
    }

    public DataRetrievalService(ILogger<DataRetrievalService> logger,
        DatahubPortalConfiguration portalConfiguration,
        NavigationManager navigationManager)
        : this(logger, null!, portalConfiguration, navigationManager)
    {
    }


    public string getBlobContainerName()
    {
        return "datahub-nrcan" + ("dev" == getEnvSuffix() ? "-dev" : "");
    }
    private string getEnvSuffix()
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        envName = envName != null ? envName.ToLower() : "dev";
        if (envName.Equals("development"))
        {
            envName = "dev";
        }

        return envName;
    }


    public async Task<Folder> GetProjectFileList(string container, string project, User user)
    {
        try
        {
            var blobServiceClient = await GetBlobServiceClient(project);
            var containerClient = blobServiceClient.GetBlobContainerClient(container);

            var folder = new Folder
            {
                id = container,
                name = container,
                createdby = "system",
                ownedby = "system"
            };
            var resultSegment = containerClient
                .GetBlobsAsync(new GetBlobsOptions { Traits = BlobTraits.Metadata })
                .AsPages(default, 30);

            // Enumerate the blobs returned for each page.
            await foreach (var blobPage in resultSegment)
            {
                foreach (var blobItem in blobPage.Values)
                {
                    // Console.WriteLine("Blob name: {0}", blobItem.Name);
                    if (!blobItem.Metadata.TryGetValue(METADATA_FILE_ID, out var fileId))
                    {
                        var newId = Guid.NewGuid().ToString();
                        blobItem.Metadata.Add(METADATA_FILE_ID, newId);
                        var client = containerClient.GetBlobClient(blobItem.Name);
                        await client.SetMetadataAsync(blobItem.Metadata);
                        fileId = newId;
                    }

                    string ownedBy = blobItem.Metadata.TryGetValue(FileMetaData.OwnedBy, out ownedBy)
                        ? ownedBy
                        : "Unknown";
                    string createdBy = blobItem.Metadata.TryGetValue(FileMetaData.CreatedBy, out createdBy)
                        ? createdBy
                        : "Unknown";
                    // string lastModifiedBy = blobItem.Metadata.TryGetValue(FileMetaData.LastModifiedBy, out lastModifiedBy) ? lastModifiedBy : "lastmodifiedby";

                    var file = new FileMetaData()
                    {
                        id = fileId,
                        filename = blobItem.Name,
                        ownedby = ownedBy,
                        createdby = createdBy,
                        // lastmodifiedby = lastModifiedBy,
                        lastmodifiedts = DateTime.Now
                    };

                    folder.Add(file, false);
                }
            }

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get file list for project: {Project} for user: {DisplayName} FAILED", project,
                user.DisplayName);
            throw;
        }
    }

    public async Task<BlobContainerClient> GetBlobContainerClient(string project, string containerName)
    {
        var blobServiceClient = await GetBlobServiceClient(project);
        return blobServiceClient.GetBlobContainerClient(containerName);
    }

    /// <summary>
    /// Gets a BlobServiceClient using managed identity/token credential (User Delegation SAS)
    /// </summary>
    private Task<BlobServiceClient> GetBlobServiceClient(string project)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        envName = envName != null ? envName.ToLower() : "dev";
        if (envName.Equals("development"))
        {
            envName = "dev";
        }

        var storageAccountName = $"dh{project}{envName}";
        var blobUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
        
        var credential = new ClientSecretCredential(
            _portalConfiguration.AzureAd.TenantId,
            _portalConfiguration.AzureAd.InfraClientId,
            _portalConfiguration.AzureAd.InfraClientSecret);
        return Task.FromResult(new BlobServiceClient(blobUri, credential));
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
        
    private async Task<Uri> GetDelegationSasBlobUri(string container, string? fileName, string projectUploadCode, int days, BlobSasPermissions permissions, bool containerLevel = false)
    {
        var project = projectUploadCode.ToLowerInvariant();
        var blobServiceClient = await GetBlobServiceClient(project);
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        
        var sasBuilder = containerLevel
            ? GetContainerSasBuild(containerClient.Name, days, permissions) 
            : GetBlobSasBuilder(container, fileName, days, permissions);

        // Use User Delegation SAS instead of Shared Key
        var userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(
            startsOn: DateTimeOffset.UtcNow,
            expiresOn: DateTimeOffset.UtcNow.AddDays(days));

        Uri uri;
        if (!string.IsNullOrEmpty(fileName))
        {
            var blobClient = containerClient.GetBlobClient(fileName);
            uri = blobClient.Uri;
        }
        else
        {
            uri = containerClient.Uri;
        }

        var storageAccountName = blobServiceClient.AccountName;
        var blobUriBuilder = new BlobUriBuilder(uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, storageAccountName)
        };

        return blobUriBuilder.ToUri();
    }

    private static BlobSasBuilder GetContainerSasBuild(string containerName, int days, BlobSasPermissions permissions)
    {
        var sasBuilder  = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            Resource = "c",
            StartsOn = DateTimeOffset.Now,
            ExpiresOn = DateTimeOffset.Now.AddDays(days)
        };
            
        sasBuilder.SetPermissions(permissions);

        return sasBuilder;
    }

    public async Task<Uri> GetUserDelegationSasBlob(string container, string fileName, string projectUploadCode, int daysValidity = 1)
    {
        return await GetDelegationSasBlobUri(container, fileName, projectUploadCode, daysValidity, BlobSasPermissions.Read | BlobSasPermissions.Write);
    }

    public async Task<Uri> GetDownloadAccessToSasBlob(string container, string fileName, string projectUploadCode, int daysValidity = 1)
    {
        return await GetDelegationSasBlobUri(container, fileName, projectUploadCode, daysValidity, BlobSasPermissions.Read);
    }

    public async Task<Uri> GenerateSasToken(string container, string projectUploadCode, int daysValidity, bool containerLevel = false)
    {
        return await GetDelegationSasBlobUri(container, null, projectUploadCode, daysValidity, BlobSasPermissions.All, containerLevel);
    }

        

    public async Task<Uri> DownloadFile(string container, FileMetaData file, string projectUploadCode)
    {
        try
        {
            if (!string.IsNullOrEmpty(projectUploadCode))
            {
                return await GetUserDelegationSasBlob(container, file.filename, projectUploadCode);
            }

            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(file.folderpath);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(file.filename);

            // Get User Delegation Key from DataLake service for User Delegation SAS
            var dataLakeServiceClient = await _dataLakeClientService.GetDataLakeServiceClient();
            var userDelegationKeyResponse = await dataLakeServiceClient.GetUserDelegationKeyAsync(
                startsOn: DateTimeOffset.UtcNow,
                expiresOn: DateTimeOffset.UtcNow.AddDays(7));
            var userDelegationKey = userDelegationKeyResponse.Value;

            DataLakeSasBuilder sasBuilder = new DataLakeSasBuilder()
            {
                FileSystemName = fileSystemClient.Name,
                Path = fileClient.Path,
                Resource = "d",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Specify read and write permissions for the SAS.
            sasBuilder.SetPermissions(DataLakeAccountSasPermissions.Read |
                                      DataLakeAccountSasPermissions.Write);

            DataLakeUriBuilder dataLakeUriBuilder = new DataLakeUriBuilder(fileClient.Uri)
            {
                // Use User Delegation Key instead of Shared Key
                Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, dataLakeServiceClient.AccountName)
            };

            _logger.LogDebug($"File URI Generation: {file.folderpath}/{file.filename} SUCCEEDED.");

            return dataLakeUriBuilder.ToUri();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"File URI Generation: {file.folderpath}/{file.filename} FAILED.");
            throw;
        }
    }

    public Task<List<Version>> GetFileVersions(string fileId)
    {
        try
        {
            // todo: get file versions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get versions for file: {FileId} FAILED", fileId);
            throw;
        }

        return Task.FromResult(new List<Version>());
    }

    public async Task<List<string>> GetSubFolders(DataLakeFileSystemClient fileSystemClient, string folderName)
    {
        var folders = new List<string>();

        try
        {
            folders.Add(folderName);

            // Now iterate through sub folders
            var directoryClient = fileSystemClient.GetDirectoryClient(folderName);
            var subdirectories = directoryClient.GetPathsAsync().AsPages(default, 20);
            await foreach (var directoryPage in subdirectories)
            {
                foreach (var item in directoryPage.Values)
                {
                    if (item.IsDirectory != null && !item.IsDirectory.Value)
                        continue;

                    var subFolders = await GetSubFolders(fileSystemClient, item.Name);
                    folders.AddRange(subFolders);
                }
            }

            _logger.LogDebug("Get all folders under folder: {FolderName} SUCCEEDED", folderName);
            return folders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get all sub folders under folder: {FolderName} FAILED", folderName);
            throw;
        }
    }

    public async Task<List<string>> GetAllFolders(string rootFolderName, User user)
    {
        try
        {
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var subFolders = await GetSubFolders(fileSystemClient, rootFolderName);

            List<string> displayableSubFolders = new List<string>();
            subFolders.ForEach(f => displayableSubFolders.Add(f.Replace(rootFolderName, @".")));

            _logger.LogDebug("Get all folders under folder: {RootFolderName} for user: {DisplayName} SUCCEEDED", rootFolderName, user.DisplayName);
            return displayableSubFolders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get all folders under folder: {RootFolderName} for user: {DisplayName} FAILED", rootFolderName, user.DisplayName);
            throw;
        }
    }

    public async Task<Folder> GetFileList(Folder folder, User user, bool onlyFolders = false, bool recursive = false)
    {
        try
        {
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(folder.fullPathFromRoot);
            var subdirectories = directoryClient.GetPathsAsync().AsPages(default, 20);

            await foreach (var directoryPage in subdirectories)
            {
                // The directoryPage.Values will contain both files and folders
                // We will ALWAYS add subfolders
                // ONLY add files IFF onlyFolders is false!
                foreach (var item in directoryPage.Values.Where(i => i.IsDirectory != null && (i.IsDirectory.Value || !onlyFolders)))
                {
                    dynamic child = Map(item, directoryClient);
                    folder.Add(child, false);

                    // If directrory and recursive, go down the tree
                    // THIS IS USUALLY ONLY FOR Folder lists!
                    if (item.IsDirectory != null && item.IsDirectory.Value && recursive)
                    {
                        await GetFileList(child, user, onlyFolders, true);
                    }
                }
            }

            folder.Sort();
            _logger.LogDebug("Get file list for folder: {FullPathFromRoot} for user: {DisplayName} results: {Count} SUCCEEDED", folder.fullPathFromRoot, user.DisplayName, folder.children.Count);

            return folder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get file list for folder: {FullPathFromRoot} for user: {DisplayName} FAILED", folder.fullPathFromRoot, user.DisplayName);
            throw;
        }
    }


    private static BaseMetadata Map(PathItem item, DataLakeDirectoryClient directoryClient)
    {
        var itemName = Path.GetFileName(item.Name);
        if (item.IsDirectory == true)
        {
            return new Folder
            {
                name = itemName,
                id = itemName,
                createdby = item.Owner ?? "system",
                ownedby = item.Owner ?? "system",
                lastmodifiedby = item.Owner ?? "system",
                lastmodifiedts = item.LastModified.DateTime
            };
        }

        DataLakeFileClient fileClient = directoryClient.GetFileClient(itemName);
        PathProperties properties = fileClient.GetProperties();
        var file = new FileMetaData()
        {
            id = Guid.NewGuid().ToString(),
            filename = itemName,
            ownedby = item.Owner ?? "system",
            createdby = item.Owner ?? "system",
            lastmodifiedby = item.Owner ?? "system",
            lastmodifiedts = item.LastModified.DateTime
        };

        file.ParseDictionary(properties.Metadata);

        return file;
    }

    public async Task<AzureStorageMetadata> GetStorageMetadata(string project)
    {
        var blobServiceClient = await GetBlobServiceClient(project?.ToLower());
        var containerClient = blobServiceClient.GetBlobContainerClient("datahub");

        var accountInfo = (await blobServiceClient.GetAccountInfoAsync()).Value;

        AzureStorageMetadata storageMetadata = new()
        {
            AccountName = blobServiceClient.AccountName,
            Container = "datahub",
            Url = containerClient.Uri.ToString(),
            Versioning = "True",
            GeoRedundancy = accountInfo.SkuName.ToString(),
            StorageAccountType = accountInfo.AccountKind.ToString()
        };

        return storageMetadata;
    }

    public async Task<List<string>> ListContainers(string projectAcronym, User user)
    {
        var blobServiceClient = await GetBlobServiceClient(projectAcronym.ToLower());
        var pages = blobServiceClient.GetBlobContainersAsync().AsPages();
        var containers = new List<string>();
        await foreach (var page in pages)
        {
            containers.AddRange(page.Values.Select(c => c.Name));
        }

        return containers;
    }

    [Obsolete("Obsolete")]
    public async Task<List<FileMetaData>> GetStorageBlobFiles(string projectAcronym, string container, User user)
    {
        try
        {
            var blobServiceClient = await GetBlobServiceClient(projectAcronym.ToLower());
            var containerClient = blobServiceClient.GetBlobContainerClient(container);

            var resultSegment = containerClient
                .GetBlobsAsync(BlobTraits.Metadata)
                .AsPages(default, 30);


            var result = new List<FileMetaData>();

            // Enumerate the blobs returned for each page.
            await foreach (var blobPage in resultSegment)
            {
                foreach (var blobItem in blobPage.Values)
                {
                    var fileId = await VerifyFileIdMetadata(blobItem, containerClient);
                    var fileMetaData = FileMetadataFromBlobItem(blobItem, fileId);
                        
                    result.Add(fileMetaData);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get file list for project: {ProjectAcronym} for user: {DisplayName} FAILED",
                projectAcronym, user.DisplayName);
            throw;
        }
    }
        
        
    [Obsolete("Use ProjectDataRetrievalService.VerifyFileIdMetadata instead")]
    private static async Task<string> VerifyFileIdMetadata(BlobHierarchyItem blobItem, BlobContainerClient containerClient)
    {
        return await VerifyFileIdMetadata(blobItem.Blob, containerClient);
    }

    [Obsolete("Use ProjectDataRetrievalService.VerifyFileIdMetadata instead")]
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

    [Obsolete("Use ProjectDataRetrievalService.GetFileMetadataFromBlobItem instead")]
    private static FileMetaData FileMetadataFromBlobItem(BlobHierarchyItem blobItem, string fileId)
    {
        return FileMetadataFromBlobItem(blobItem.Blob, fileId);
    }

    [Obsolete("Use ProjectDataRetrievalService.GetFileMetadataFromBlobItem instead")]
    private static FileMetaData FileMetadataFromBlobItem(BlobItem blobItem, string fileId)
    {
        string ownedBy = blobItem.Metadata.TryGetValue(FileMetaData.OwnedBy, out ownedBy) ? ownedBy : "Unknown";
        string createdBy = blobItem.Metadata.TryGetValue(FileMetaData.CreatedBy, out createdBy)
            ? createdBy
            : "Unknown";
        string lastModifiedBy = blobItem.Metadata.TryGetValue(FileMetaData.LastModifiedBy, out lastModifiedBy)
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

        string lastModified = blobItem.Metadata.TryGetValue(FileMetaData.LastModified, out lastModified)
            ? lastModified
            : DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        string fileSize = blobItem.Metadata.TryGetValue(FileMetaData.FileSize, out fileSize) ? fileSize : "0";

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

    [Obsolete("Obsolete")]
    public async Task<(List<string>, List<FileMetaData>, string)> GetStorageBlobPagesAsync(string projectAcronym, string containerName, User user, string prefix, string? continuationToken = default)
    {
        try
        {
            var folders = new List<string>();
            var files = new List<FileMetaData>();
                
            var blobServiceClient = await GetBlobServiceClient(projectAcronym.ToLower());
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

    
    public async Task<List<string>> GetProjectContainersAsync(string projectAcronymParam, User user)
    {
        return await ListContainers(projectAcronymParam, user);
    }

    public async Task<bool> StorageBlobExistsAsync(string filename, string projectAcronym, string containerName)
    {
        var blobServiceClient = await GetBlobServiceClient(projectAcronym.ToLower());
            
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(filename);

        return await blobClient.ExistsAsync();
    }
}
