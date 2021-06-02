using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace NRCan.Datahub.Portal.Services
{
    public class DataRetrievalService : BaseService, IDataRetrievalService
    {
        private ILogger<DataRetrievalService> _logger;
        private IHttpClientFactory _httpClient;
        private IUserInformationService _userInformationService;
        private IKeyVaultService _keyVaultService;
        private ApiCallService _apiCallService;
        private DataLakeClientService _dataLakeClientService;
        private IJSRuntime _jsRuntime;
        private CommonAzureServices _commonAzureServices;
        private ICognitiveSearchService _cognitiveSearchService;

        public DataRetrievalService(ILogger<DataRetrievalService> logger,
                                    IHttpClientFactory clientFactory,
                                    IUserInformationService userInformationService,
                                    IKeyVaultService keyVaultService,
                                    ApiCallService apiCallService,
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
                return await _apiService.GetFileList(folder, user, onlyFolders, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetFolderStructure folder: {folder.fullPathFromRoot} user: {user.DisplayName} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return folder;
        }

        public async Task<Folder> GetFolderContents(dynamic folder, string filterSearch, Microsoft.Graph.User user)
        {
            try
            {
                // Clear folder as we will reload!
                folder.Clear();
                if (!string.IsNullOrWhiteSpace(filterSearch))
                {
                    return await getSearchResults(folder, filterSearch, user);
                }

                if (folder.isShared)
                {
                    return await getSharedFileList(folder, user);
                }

                return await _apiService.GetFileList(folder, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetFileList folder: {folder.fullPathFromRoot} filter search: {filterSearch} user: {user.DisplayName} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return folder;
        }

        public async Task DownloadFile(FileMetaData file)
        {
            try
            {
                await _apiService.DownloadFile(file);
            }
            catch (Exception ex)
            {
                base.DisplayErrorUI(ex);
            }
        }

        public async Task<List<Shared.Data.Version>> GetFileVersions(string fileId)
        {
            try
            {
                List<Shared.Data.Version> versions = new List<Shared.Data.Version>();
                return versions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get versions for file: {fileId} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return new List<Shared.Data.Version>();
        }

        public async Task<List<string>> GetSubFolders(DataLakeFileSystemClient fileSystemClient, string folderName)
        {
            List<string> folders = new List<string>();

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
                base.DisplayErrorUI(ex);
            }

            return new List<string>();
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
                base.DisplayErrorUI(ex);
            }

            return new List<string>();
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
    }
}
