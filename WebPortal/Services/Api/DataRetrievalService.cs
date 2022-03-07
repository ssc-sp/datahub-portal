using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Core.Data;
using Datahub.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Graph;
using Folder = Datahub.Core.Data.Folder;

namespace Datahub.Portal.Services.Api
{
    public class DataRetrievalService : BaseService, IDataRetrievalService
    {
        private const string METADATA_FILE_ID = "fileid";
        private readonly ILogger<DataRetrievalService> _logger;
        private readonly IApiCallService _apiCallService;
        private readonly DataLakeClientService _dataLakeClientService;

        public DataRetrievalService(ILogger<DataRetrievalService> logger,
                                    IApiCallService apiCallService,
                                    IApiService apiService,
                                    DataLakeClientService dataLakeClientService,
                                    NavigationManager navigationManager,
                                    UIControlsService uiService)
            : base(navigationManager, apiService, uiService)
        {
            _logger = logger;
            _apiCallService = apiCallService;
            _dataLakeClientService = dataLakeClientService;
        }


        public async Task<Folder> GetFolderStructure(Folder folder, User user, bool onlyFolders = true)
        {
            try
            {
                return await GetFileList(folder, user, onlyFolders, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFolderStructure folder: {FullPathFromRoot} user: {DisplayName} FAILED", folder.fullPathFromRoot, user.DisplayName);
                throw;
            }
        }

        public async Task<Folder> GetFolderContents(Folder folder, string filterSearch, User user, string project = null)
        {
            try
            {
                // Clear folder as we will reload!
                folder?.Clear();
                if (!string.IsNullOrWhiteSpace(filterSearch))
                {
                    return await GetSearchResults(folder, filterSearch, user);
                }

                if (folder?.isShared ?? false)
                {
                    return await GetSharedFileList(folder, user);
                }

                if (!string.IsNullOrEmpty(project))
                {
                    return await GetProjectFileList(project, user);
                }

                return await GetFileList(folder, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFileList folder: {FullPathFromRoot} filter search: {FilterSearch} user: {DisplayName} FAILED",
                    folder?.fullPathFromRoot, filterSearch, user?.DisplayName);
                throw;
            }
        }

        private async Task<Folder> GetProjectFileList(string project, User user)
        {
            try
            {

                var connectionString = await _apiCallService.GetProjectConnectionString(project);
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("datahub");

                var folder = new Folder();
                var resultSegment = containerClient
                    .GetBlobsAsync(BlobTraits.Metadata)
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
                        
                        string ownedBy = blobItem.Metadata.TryGetValue(FileMetaData.OwnedBy, out ownedBy) ? ownedBy : "Unknown";
                        string createdBy = blobItem.Metadata.TryGetValue(FileMetaData.CreatedBy, out createdBy) ? createdBy : "Unknown";
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
                _logger.LogError(ex, "Get file list for project: {Project} for user: {DisplayName} FAILED", project, user.DisplayName);
                throw;
            }
        }


        public async Task<Uri> DownloadFile(FileMetaData file, string project)
        {
            return await _apiService.DownloadFile(file, project);
        }

        public Task<List<Core.Data.Version>> GetFileVersions(string fileId)
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

            return Task.FromResult(new List<Core.Data.Version>());
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

        private async Task<Folder> GetSharedFileList(dynamic folder, User user)
        {
            var filter = $"sharedwith/any(c: c/userid eq '{user.Id}')";
            return await _apiService.SearchIndex(folder, filter, user);
        }

        private async Task<Folder> GetSearchResults(dynamic folder, string searchText, User user)
        {
            var filter = $"search.ismatch('{searchText}*', 'filename', 'full', 'any') and ownedby eq '{user.Id}'";
            return await _apiService.SearchIndex(folder, filter, user);
        }

        private async Task<Folder> GetFileList(Folder folder, User user, bool onlyFolders = false, bool recursive = false)
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
                    createdby = item.Owner,
                    lastmodifiedby = item.Owner,
                    lastmodifiedts = item.LastModified.DateTime
                };
            }

            DataLakeFileClient fileClient = directoryClient.GetFileClient(itemName);
            PathProperties properties = fileClient.GetProperties();
            var file = new FileMetaData()
            {
                filename = itemName,
                ownedby = item.Owner,
                createdby = item.Owner,
                lastmodifiedby = item.Owner,
                lastmodifiedts = item.LastModified.DateTime
            };

            file.ParseDictionary(properties.Metadata);

            return file;
        }

        public async Task<StorageMetadata> GetStorageMetadata(string project)
        {
            string cxnstring = await _apiCallService.GetProjectConnectionString(project);
            BlobServiceClient blobServiceClient = new(cxnstring);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("datahub");

            var accountInfo = blobServiceClient.GetAccountInfo().Value;
            //var accountype = accountInfo.Value
            //var skuName = accountInfo.SkuName;

            StorageMetadata storeageMetadata = new() 
            {
                Container = "datahub",
                Url = containerClient.Uri.ToString(),
                Versioning = "True",
                GeoRedundancy = accountInfo.SkuName.ToString(),
                StorageAccountType = accountInfo.AccountKind.ToString()
            };

            return storeageMetadata;
        }

        public async Task<List<FileMetaData>> GetStorageBlobFiles(string projectAcronym, User user)
        {
            try
            {
                var connectionString = await _apiCallService.GetProjectConnectionString(projectAcronym.ToLower());
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("datahub");

                var resultSegment = containerClient
                    .GetBlobsAsync(BlobTraits.Metadata)
                    .AsPages(default, 30);


                var result = new List<FileMetaData>();
                
                // Enumerate the blobs returned for each page.
                await foreach (var blobPage in resultSegment)
                {
                    foreach (var blobItem in blobPage.Values)
                    {
                        if (!blobItem.Metadata.TryGetValue(METADATA_FILE_ID, out var fileId))
                        {
                            var newId = Guid.NewGuid().ToString();
                            blobItem.Metadata.Add(METADATA_FILE_ID, newId);
                            var client = containerClient.GetBlobClient(blobItem.Name);
                            await client.SetMetadataAsync(blobItem.Metadata);
                            fileId = newId;
                            
                        }
                        
                        string ownedBy = blobItem.Metadata.TryGetValue(FileMetaData.OwnedBy, out ownedBy) ? ownedBy : "Unknown";
                        string createdBy = blobItem.Metadata.TryGetValue(FileMetaData.CreatedBy, out createdBy) ? createdBy : "Unknown";
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
                        
                        result.Add(file);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get file list for project: {ProjectAcronym} for user: {DisplayName} FAILED", projectAcronym, user.DisplayName);
                throw;
            }
        }
    }
}
