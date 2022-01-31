using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Datahub.Core.Data;
using Datahub.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Datahub.Portal.Services
{
    public class DataRetrievalService : BaseService, IDataRetrievalService
    {
        private ILogger<DataRetrievalService> _logger;
        private IHttpClientFactory _httpClient;
        private IUserInformationService _userInformationService;
        private IKeyVaultService _keyVaultService;
        private IApiCallService _apiCallService;
        private DataLakeClientService _dataLakeClientService;
        private IJSRuntime _jsRuntime;
        private CommonAzureServices _commonAzureServices;
        private ICognitiveSearchService _cognitiveSearchService;

        public DataRetrievalService(ILogger<DataRetrievalService> logger,
                                    IHttpClientFactory clientFactory,
                                    IUserInformationService userInformationService,
                                    IKeyVaultService keyVaultService,
                                    IApiCallService apiCallService,
                                    IApiService apiService,
                                    DataLakeClientService dataLakeClientService,
                                    ICognitiveSearchService cognitiveSearchService,
                                    CommonAzureServices commonAzureServices,
                                    IJSRuntime jSRuntime,
                                    NavigationManager navigationManager,
                                    UIControlsService uiService)
            : base(navigationManager, apiService, uiService)
        {
            _logger = logger;
            _httpClient = clientFactory;
            _userInformationService = userInformationService;
            _keyVaultService = keyVaultService;
            _apiCallService = apiCallService;
            _dataLakeClientService = dataLakeClientService;
            _jsRuntime = jSRuntime;
            _commonAzureServices = commonAzureServices;
            _cognitiveSearchService = cognitiveSearchService;
        }


        public async Task<Folder> GetFolderStructure(Folder folder, Microsoft.Graph.User user, bool onlyFolders = true)
        {
            try
            {
                return await GetFileList(folder, user, onlyFolders, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetFolderStructure folder: {folder.fullPathFromRoot} user: {user.DisplayName} FAILED.");
                throw;
            }
        }

        public async Task<Folder> GetFolderContents(Folder folder, string filterSearch, Microsoft.Graph.User user, string project = null)
        {
            try
            {
                // Clear folder as we will reload!
                folder?.Clear();
                if (!string.IsNullOrWhiteSpace(filterSearch))
                {
                    return await getSearchResults(folder, filterSearch, user);
                }

                if (folder?.isShared ?? false)
                {
                    return await getSharedFileList(folder, user);
                }

                if (!string.IsNullOrEmpty(project))
                {
                    return await GetProjectFileList(project, user);
                }

                return await GetFileList(folder, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetFileList folder: {folder?.fullPathFromRoot} filter search: {filterSearch} user: {user?.DisplayName} FAILED.");
                throw;
            }
        }

        private async Task<Folder> GetProjectFileList(string project, Microsoft.Graph.User user)
        {
            try
            {

                string cxnstring = await _apiCallService.GetProjectConnectionString(project);
                BlobServiceClient blobServiceClient = new BlobServiceClient(cxnstring);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("datahub");

                Folder folder = new Folder();
                var resultSegment = containerClient.GetBlobsAsync(BlobTraits.Metadata)
                .AsPages(default, 30);

                // Enumerate the blobs returned for each page.
                await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blobItem in blobPage.Values)
                    {
                        
                        Console.WriteLine("Blob name: {0}", blobItem.Name);
                        string fileId = blobItem.Metadata.TryGetValue("fileid", out fileId) ? fileId : "External";
                        string ownedby = blobItem.Metadata.TryGetValue("ownedby", out ownedby) ? ownedby : "Unknown";
                        string createdby = blobItem.Metadata.TryGetValue("createdby", out createdby) ? createdby : "Unknown";
                        string lastmodifiedby = blobItem.Metadata.TryGetValue("lastmodifiedby", out lastmodifiedby) ? lastmodifiedby : "lastmodifiedby";

                        var file = new FileMetaData()
                        {
                            id = fileId,
                            filename = blobItem.Name,
                            ownedby = ownedby,
                            createdby = createdby,
                            lastmodifiedby = lastmodifiedby,
                            lastmodifiedts = DateTime.Now
                        };
                        folder.Add(file, false);
                    }


                }

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get file list for project: {project} for user: {user.DisplayName} FAILED.");
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
                _logger.LogError(ex, $"Get versions for file: {fileId} FAILED.");
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
                await foreach (Azure.Page<PathItem> directoryPage in subdirectories)
                {
                    foreach (var item in directoryPage.Values)
                    {
                        if (item.IsDirectory.Value)
                        {
                            var subFolders = await GetSubFolders(fileSystemClient, item.Name);
                            folders.AddRange(subFolders);
                        }
                    }
                }

                _logger.LogDebug($"Get all folders under folder: {folderName} SUCCEEDED.");
                return folders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get all sub folders under folder: {folderName} FAILED.");
                throw;
            }
        }

        public async Task<List<string>> GetAllFolders(string rootFolderName, Microsoft.Graph.User user)
        {
            try
            {
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var subFolders = await GetSubFolders(fileSystemClient, rootFolderName);

                List<string> displayableSubFolders = new List<string>();
                subFolders.ForEach(f => displayableSubFolders.Add(f.Replace(rootFolderName, @".")));

                _logger.LogDebug($"Get all folders under folder: {rootFolderName} for user: {user.DisplayName} SUCCEEDED.");
                return displayableSubFolders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get all folders under folder: {rootFolderName} for user: {user.DisplayName} FAILED.");
                throw;
            }
        }

        protected async Task<Folder> getSharedFileList(dynamic folder, Microsoft.Graph.User user)
        {
            string filter = $"sharedwith/any(c: c/userid eq '{user.Id}')";
            return await _apiService.SearchIndex(folder, filter, user);
        }

        protected async Task<Folder> getSearchResults(dynamic folder, string searchText, Microsoft.Graph.User user)
        {
            string filter = $"search.ismatch('{searchText}*', 'filename', 'full', 'any') and ownedby eq '{user.Id}'";
            return await _apiService.SearchIndex(folder, filter, user);
        }

        public async Task<Folder> GetFileList(Folder folder, Microsoft.Graph.User user, bool onlyFolders = false, bool recursive = false)
        {
            try
            {
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(folder.fullPathFromRoot);
                var subdirectories = directoryClient.GetPathsAsync().AsPages(default, 20);

                await foreach (Azure.Page<PathItem> directoryPage in subdirectories)
                {
                    // The directoryPage.Values will contain both files and folders
                    // We will ALWAYS add subfolders
                    // ONLY add files IFF onlyFolders is false!
                    foreach (var item in directoryPage.Values.Where(i => i.IsDirectory.Value || !onlyFolders))
                    {
                        dynamic child = Map(item, directoryClient);
                        folder.Add(child, false);

                        // If directrory and recursive, go down the tree
                        // THIS IS USUALLY ONLY FOR Folder lists!
                        if (item.IsDirectory.Value && recursive)
                        {
                            child = await GetFileList(child, user, onlyFolders, recursive);
                        }
                    }
                }

                folder.Sort();
                _logger.LogDebug($"Get file list for folder: {folder.fullPathFromRoot} for user: {user.DisplayName} results: {folder.children.Count} SUCCEEDED.");

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get file list for folder: {folder.fullPathFromRoot} for user: {user.DisplayName} FAILED.");
                throw;
            }
        }


        protected BaseMetadata Map(PathItem item, DataLakeDirectoryClient directoryClient)
        {
            var itemName = System.IO.Path.GetFileName(item.Name);
            if (item.IsDirectory == true)
            {
                return new Folder()
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
    }
}
