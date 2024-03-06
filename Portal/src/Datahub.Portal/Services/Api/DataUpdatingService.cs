﻿using Azure.Storage.Files.DataLake;
using Datahub.Core.Data;
using Datahub.Core.Services;
using Datahub.Core.Services.Api;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Folder = Datahub.Core.Data.Folder;

namespace Datahub.Portal.Services.Api;

/// <summary>
/// Important note - This service is not used but is kept because it has the code
/// to work with Gen2 storage and ACL
/// </summary>
public class DataUpdatingService : BaseService
{
    private readonly ILogger<DataUpdatingService> _logger;
    private readonly IHttpClientFactory _httpClient;
    private readonly IUserInformationService _userInformationService;
    private readonly DataLakeClientService _dataLakeClientService;
    private readonly DataRetrievalService _dataRetrievalService;

    public DataUpdatingService(ILogger<DataUpdatingService> logger,
        IHttpClientFactory clientFactory,
        IUserInformationService userInformationService,
        DataLakeClientService dataLakeClientService,
        DataRetrievalService dataRetrievalService,
        NavigationManager navigationManager)
        : base(navigationManager)
    {
        _logger = logger;
        _httpClient = clientFactory;
        _userInformationService = userInformationService;
        _dataLakeClientService = dataLakeClientService;
        _dataRetrievalService = dataRetrievalService;
    }

    public async Task<bool> RenameFolder(Folder folder, string newFolderName, Microsoft.Graph.Models.User currentUser)
    {
        try
        {
            // Rename this folder
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(folder.FullPathFromRoot);
            var respsonse = await directoryClient.RenameAsync($"{folder.Parent.FullPathFromRoot}/{newFolderName}");

            folder.Name = newFolderName;
            folder.Id = newFolderName;

            // Because we may have files in this or sub folders of this folder
            // We need to update their folderpaths...
            //folder = await myDataService.GetFolderStructure(folder, currentUser, false);

            await UpdateFilesWithNewFolderPath(fileSystemClient, folder, currentUser);

            _logger.LogDebug($"Renamed folder: {folder.FullPathFromRoot} to {folder.Parent.FullPathFromRoot}/{newFolderName} for user: {currentUser.DisplayName} SUCCEEDED.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Renamed folder: {folder.FullPathFromRoot} to {folder.Parent.FullPathFromRoot}/{newFolderName} for user: {currentUser.DisplayName} FAILED.");
            throw;
        }
    }

    public async Task RenameStorageBlob(string oldName, string newName, string projectAcronym, string containerName)
    {
        var connectionString = await _dataRetrievalService.GetProjectConnectionString(projectAcronym.ToLower());
        var container = CloudStorageAccount.Parse(connectionString)
            .CreateCloudBlobClient()
            .GetContainerReference(containerName);

        var source = (CloudBlockBlob)await container.GetBlobReferenceFromServerAsync(oldName);
        var target = container.GetBlockBlobReference(newName);

        if (source is null)
        {
            throw new Exception($"Could not find blob: {oldName}");
        }

        await target.StartCopyAsync(source);

        while (target.CopyState.Status == CopyStatus.Pending)
            await Task.Delay(100);

        if (target.CopyState.Status != CopyStatus.Success)
            throw new Exception("Rename failed: " + target.CopyState.Status);

        await source.DeleteAsync();
    }

    public async Task<bool> RenameFile(FileMetaData file, string newFileName, Microsoft.Graph.Models.User currentUser)
    {
        var oldFile = $"{file.Folderpath}/{file.Filename}";
        try
        {
            // Same folder, new file name
            return await RenameDataLakeFile(file, file.Folderpath, newFileName, currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Rename file from: {oldFile} to: {file.Folderpath}/{newFileName} User: {currentUser.DisplayName} FAILED.");
            throw;
        }
    }

    public async Task<bool> MoveFile(FileMetaData file, string newParentFolder, Microsoft.Graph.Models.User currentUser)
    {
        var oldFile = $"{file.Folderpath}/{file.Filename}";
        try
        {
            // new folder path, same filename
            return await RenameDataLakeFile(file, newParentFolder, file.Filename, currentUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Move file from: {oldFile} to: {newParentFolder}/{file.Filename} User: {currentUser.DisplayName} FAILED.");
            throw;
        }
    }

    protected async Task UpdateFilesWithNewFolderPath(DataLakeFileSystemClient fileSystemClient, Folder folder, Microsoft.Graph.Models.User currentUser)
    {
        if (folder != null)
        {
            try
            {
                var directoryClient = fileSystemClient.GetDirectoryClient(folder.FullPathFromRoot);

                // Iterate thru files and alter folderpath to crrect values
                foreach (var file in folder.AllFiles)
                {
                    var fileClient = directoryClient.GetFileClient(file.Filename);
                    UpdateFileFolderPath(fileClient, file, currentUser);
                }

                // Iterate down sub folders
                foreach (Folder subFolder in folder.SubFolders)
                {
                    await UpdateFilesWithNewFolderPath(fileSystemClient, subFolder, currentUser);
                }

                _logger.LogDebug($"UpdateFilesWithNewFolderPath folder: {folder.FullPathFromRoot} for user: {currentUser.DisplayName} SUCCEEDED.");
            }
            catch (Exception ex)
            {
                // We eat thhis exception, possible other folders/files will succeed!
                _logger.LogError(ex, $"UpdateFilesWithNewFolderPath folder: {folder.FullPathFromRoot} for user: {currentUser.DisplayName} FAILED.");
            }
        }
    }

    private void UpdateFileFolderPath(DataLakeFileClient fileClient, FileMetaData file, Microsoft.Graph.Models.User currentUser)
    {
        var oldFolderpath = file.Folderpath;

        try
        {
            file.Lastmodifiedts = DateTime.UtcNow;
            file.Lastmodifiedby = currentUser.Id;
            file.Folderpath = file.Parent.FullPathFromRoot;

            fileClient.SetMetadata(file.GenerateMetadata());

            //await _cognitiveSearchService.EditDocument(file);

            _logger.LogDebug($"File's parent folder path changed from: {oldFolderpath} to: {file.Parent.FullPathFromRoot} User: {currentUser.DisplayName} SUCCEEDED.");
        }
        catch (Exception ex)
        {
            // We eat thhis exception, possible other files will succeed!
            _logger.LogError(ex, $"File's parent folder path changed from: {oldFolderpath} to: {file.Parent.FullPathFromRoot} User: {currentUser.DisplayName} FAILED.");
        }
    }

    protected async Task<bool> RenameDataLakeFile(FileMetaData file, string newFolderPath, string newFileName, Microsoft.Graph.Models.User currentUser)
    {
        try
        {
            // Get the original file!
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(file.Folderpath);
            var fileClient = directoryClient.GetFileClient(file.Filename);

            var response = await fileClient.RenameAsync($"{newFolderPath}/{newFileName}");
            if (response.Value != null)
            {
                _logger.LogDebug($"Renamed file from: {file.Folderpath}/{file.Filename} to: {newFolderPath}/{newFileName} User: {currentUser.DisplayName} SUCCEEDED.");

                // File renamed update our metadata
                file.Folderpath = newFolderPath;
                file.Filename = newFileName;
                file.Lastmodifiedts = DateTime.UtcNow;
                file.Lastmodifiedby = currentUser.Id;
                response.Value.SetMetadata(file.GenerateMetadata());

                //await _cognitiveSearchService.EditDocument(file);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Renamed file from: {file.Folderpath}/{file.Filename} to: {newFolderPath}/{newFileName} User: {currentUser.DisplayName} FAILED.");
            throw;
        }
    }


}