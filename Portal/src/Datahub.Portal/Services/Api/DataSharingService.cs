﻿using Azure.Storage.Files.DataLake.Models;
using Datahub.Core.Data;
using Datahub.Core.Services.Api;
using Datahub.Core.Services.Data;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Portal.Model;
using Microsoft.AspNetCore.Components;

namespace Datahub.Portal.Services.Api;

public class DataSharingService : BaseService, IDataSharingService
{
    private ILogger<DataSharingService> _logger;
    private DataLakeClientService _dataLakeClientService;
    public DataSharingService(ILogger<DataSharingService> logger,
        DataLakeClientService dataLakeClientService,
        NavigationManager navigationManager)
        : base(navigationManager)
    {
        _logger = logger;
        _dataLakeClientService = dataLakeClientService;
    }

    public async Task<bool> ChangeFileOwner(FileMetaData file, GraphUser newOwner, string currentUserId)
    {
        try
        {
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(file.Folderpath);
            var fileClient = directoryClient.GetFileClient(file.Filename);

            // STEP 1: If new owner has ACL, remove it (CUZ THEY GONNA OWN IT!)
            if (file.Sharedwith.Any(s => s.Userid == newOwner.Id))
            {
                await RemoveSharedUsers(file, newOwner.Id);
            }

            // STEP 2: Change to new owner
            PathPermissions perm = new PathPermissions()
            {
                Owner = RolePermissions.Read | RolePermissions.Write
            };

            await fileClient.SetPermissionsAsync(perm, newOwner.Id);
            file.Ownedby = newOwner.Id;
            file.Lastmodifiedts = DateTime.UtcNow;
            fileClient.SetMetadata(file.GenerateMetadata());

            //await _cognitiveSearchService.EditDocument(file);

            // STEP 3: Assign us back as a shared user
            await AddSharedUsers(file, currentUserId, AccessPermissions.Editor);

            // STEP 4: Move the file to the root folder of new owner!
            var response = await fileClient.RenameAsync($"{newOwner.RootFolder}/{file.Filename}");
            if (response.Value != null)
            {
                _logger.LogDebug($"Changed File Owner for file: {file.Folderpath}/{file.Filename} from user: {currentUserId} to user: {newOwner.DisplayName} SUCCEEDED.");
                return true;
            }

            _logger.LogDebug($"Changed File Owner for file: {file.Folderpath}/{file.Filename} from user: {currentUserId} to user: {newOwner.DisplayName} FAILED (nothing returned).");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Changed File Owner for file: {file.Folderpath}/{file.Filename} from user: {currentUserId} to user: {newOwner.DisplayName} FAILED.");
            throw;
        }
    }

    public async Task<bool> AddSharedUsers(FileMetaData file, string sharedUserId, string role)
    {
        try
        {
            var permissions = role == AccessPermissions.Viewer ? "r--" : "rw-";
            var result = await _dataLakeClientService.AssignOwnerPermissionsToFile(file, sharedUserId, permissions);

            var status = result ? "SUCCEEDED" : "FAILED";
            _logger.LogDebug($"Added shared user for file: {file.Folderpath}/{file.Filename} for user: {sharedUserId} with role: {role} {status}.");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Added shared user for file: {file.Folderpath}/{file.Filename} for user: {sharedUserId} with role: {role} FAILED.");
            throw;
        }
    }

    public async Task<bool> RemoveSharedUsers(FileMetaData file, string sharedUserId)
    {
        try
        {
            var result = await _dataLakeClientService.RemoveSharedUser(file, sharedUserId);
            var status = result ? "SUCCEEDED" : "FAILED";
            _logger.LogDebug($"Removed shared user for file: {file.Folderpath}/{file.Filename} for user: {sharedUserId} {status}.");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Removed shared user for file: {file.Folderpath}/{file.Filename} for user: {sharedUserId} FAILED.");
            throw;
        }
    }

    public async Task LoadSharedUsers(FileMetaData file)
    {
        try
        {
            await _dataLakeClientService.LoadSharedUsers(file);
            _logger.LogDebug($"Loaded shared users for file: {file.Folderpath}/{file.Filename} SUCCEEDED.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Loaded shared users for file: {file.Folderpath}/{file.Filename} FAILED.");
            throw;
        }
    }
}