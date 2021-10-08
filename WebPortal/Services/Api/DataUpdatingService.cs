using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Datahub.Shared.Data;
using Datahub.Shared.Services;

namespace Datahub.Portal.Services
{
    public class DataUpdatingService : BaseService, IDataUpdatingService
    {
        private ILogger<DataUpdatingService> _logger;
        private IHttpClientFactory _httpClient;
        private IUserInformationService _userInformationService;
        private IApiCallService _apiCallService;
        private DataLakeClientService _dataLakeClientService;
        private IDataRetrievalService _retrievalService;

        private ICognitiveSearchService _cognitiveSearchService;

        public DataUpdatingService(ILogger<DataUpdatingService> logger,
                    IHttpClientFactory clientFactory,
                    IUserInformationService userInformationService,
                    DataLakeClientService dataLakeClientService,
                    ICognitiveSearchService cognitiveSearchService,
                    IApiService apiService,
                    IDataRetrievalService retrievalService,
                    IApiCallService apiCallService,
                    NavigationManager navigationManager,
                    UIControlsService uiService)
            : base(navigationManager, apiService, uiService)
        {
            _logger = logger;
            _httpClient = clientFactory;
            _userInformationService = userInformationService;
            _apiCallService = apiCallService;
            _cognitiveSearchService = cognitiveSearchService;
            _dataLakeClientService = dataLakeClientService;
            _retrievalService = retrievalService;
        }

        public async Task<bool> RenameFolder(Folder folder, string newFolderName, Microsoft.Graph.User currentUser)
        {
            try
            {
                // Rename this folder
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(folder.fullPathFromRoot);
                var respsonse = await directoryClient.RenameAsync($"{folder.parent.fullPathFromRoot}/{newFolderName}");

                folder.name = newFolderName;
                folder.id = newFolderName;

                // Because we may have files in this or sub folders of this folder
                // We need to update their folderpaths...
                folder = await _retrievalService.GetFolderStructure(folder, currentUser, false);

                await UpdateFilesWithNewFolderPath(fileSystemClient, folder, currentUser);

                _logger.LogDebug($"Renamed folder: {folder.fullPathFromRoot} to {folder.parent.fullPathFromRoot}/{newFolderName} for user: {currentUser.DisplayName} SUCCEEDED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Renamed folder: {folder.fullPathFromRoot} to {folder.parent.fullPathFromRoot}/{newFolderName} for user: {currentUser.DisplayName} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return false;
        }

        public async Task<bool> RenameFile(FileMetaData file, string newFileName, Microsoft.Graph.User currentUser)
        {
            var oldFile = $"{file.folderpath}/{file.filename}";
            try
            {
                // Same folder, new file name
                return await ChangeFile(file, file.folderpath, newFileName, currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rename file from: {oldFile} to: {file.folderpath}/{newFileName} User: {currentUser.DisplayName} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return false;
        }

        public async Task<bool> MoveFile(FileMetaData file, string newParentFolder, Microsoft.Graph.User currentUser)
        {
            var oldFile = $"{file.folderpath}/{file.filename}";

            try
            {
                // new folder path, same filename
                return await ChangeFile(file, newParentFolder, file.filename, currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Move file from: {oldFile} to: {newParentFolder}/{file.filename} User: {currentUser.DisplayName} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return false;
        }

        protected async Task UpdateFilesWithNewFolderPath(DataLakeFileSystemClient fileSystemClient, Folder folder, Microsoft.Graph.User currentUser)
        {
            if (folder != null)
            {
                try
                {
                    var directoryClient = fileSystemClient.GetDirectoryClient(folder.fullPathFromRoot);

                    // Iterate thru files and alter folderpath to crrect values
                    foreach (var file in folder.AllFiles)
                    {
                        var fileClient = directoryClient.GetFileClient(file.filename);
                        await UpdateFileFolderPath(fileClient, file, currentUser);
                    }

                    // Iterate down sub folders
                    foreach (Folder subFolder in folder.SubFolders)
                    {
                        await UpdateFilesWithNewFolderPath(fileSystemClient, subFolder, currentUser);
                    }

                    _logger.LogDebug($"UpdateFilesWithNewFolderPath folder: {folder.fullPathFromRoot} for user: {currentUser.DisplayName} SUCCEEDED.");
                }
                catch (Exception ex)
                {
                    // We eat thhis exception, possible other folders/files will succeed!
                    _logger.LogError(ex, $"UpdateFilesWithNewFolderPath folder: {folder.fullPathFromRoot} for user: {currentUser.DisplayName} FAILED.");
                }
            }
        }

        protected async Task UpdateFileFolderPath(DataLakeFileClient fileClient, FileMetaData file, Microsoft.Graph.User currentUser)
        {
            var oldFolderpath = file.folderpath;

            try
            {
                file.lastmodifiedts = DateTime.UtcNow;
                file.lastmodifiedby = currentUser.Id;
                file.folderpath = file.parent.fullPathFromRoot;

                fileClient.SetMetadata(file.GenerateMetadata());

                await _cognitiveSearchService.EditDocument(file);

                _logger.LogDebug($"File's parent folder path changed from: {oldFolderpath} to: {file.parent.fullPathFromRoot} User: {currentUser.DisplayName} SUCCEEDED.");
            }
            catch (Exception ex)
            {
                // We eat thhis exception, possible other files will succeed!
                _logger.LogError(ex, $"File's parent folder path changed from: {oldFolderpath} to: {file.parent.fullPathFromRoot} User: {currentUser.DisplayName} FAILED.");
            }
        }

        protected async Task<bool> ChangeFile(FileMetaData file, string newFolderPath, string newFileName, Microsoft.Graph.User currentUser)
        {
            try
            {
                // Get the original file!
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(file.folderpath);
                var fileClient = directoryClient.GetFileClient(file.filename);

                var response = await fileClient.RenameAsync($"{newFolderPath}/{newFileName}");
                if (response.Value != null)
                {
                    _logger.LogDebug($"Renamed file from: {file.folderpath}/{file.filename} to: {newFolderPath}/{newFileName} User: {currentUser.DisplayName} SUCCEEDED.");

                    // File renamed update our metadata
                    file.folderpath = newFolderPath;
                    file.filename = newFileName;
                    file.lastmodifiedts = DateTime.UtcNow;
                    file.lastmodifiedby = currentUser.Id;
                    response.Value.SetMetadata(file.GenerateMetadata());

                    await _cognitiveSearchService.EditDocument(file);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Renamed file from: {file.folderpath}/{file.filename} to: {newFolderPath}/{newFileName} User: {currentUser.DisplayName} FAILED.");
                throw;
            }
        }
    }
}
