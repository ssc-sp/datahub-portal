using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Datahub.Shared.Data;
using Datahub.Shared.Services;
using System;
using System.Threading.Tasks;

namespace Datahub.Portal.Services
{
    public class DataCreatorService : BaseService, IDataCreatorService
    {        
        private ILogger<DataCreatorService> _logger;
        private DataLakeClientService _dataLakeClientService;

        public DataCreatorService(ILogger<DataCreatorService> logger,
                                  IApiService apiService,
                                  DataLakeClientService dataLakeClientService,
                                  NavigationManager navigationManager,
                                  UIControlsService uiService)
            : base(navigationManager, apiService, uiService)
        {
            _logger = logger;
            _dataLakeClientService = dataLakeClientService;
        }

        public async Task<bool> CreateFolder(Folder folder, Folder parent, Microsoft.Graph.User user)
        {
            var folderName = $"{parent.fullPathFromRoot}/{folder.id}";

            try
            {              
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();

                var directoryClient = await fileSystemClient.CreateDirectoryAsync(folderName);
                await SetDefaultFolderPermissions(directoryClient, folderName, user.Id);

                _logger.LogDebug($"CreateFolder: {folderName} user: {user.DisplayName} SUCCEEDED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateFolder: {folderName} user: {user.DisplayName} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return false;          
        }

        public async Task<bool> CreateRootFolderIfNotExist(string userId, string rootFolder)
        {
            try
            {
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(rootFolder);

                await directoryClient.CreateIfNotExistsAsync();
                await SetDefaultFolderPermissions(directoryClient, rootFolder, userId);

                _logger.LogDebug($"CreateRootFolderIfNotExist: {rootFolder}  user: {userId} SUCCEEDED.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateRootFolderIfNotExist: {rootFolder}  user: {userId} FAILED.");
                base.DisplayErrorUI(ex);
            }

            return false;          
        }

        protected async Task SetDefaultFolderPermissions(DataLakeDirectoryClient directoryClient, string folderName, string userId)
        {
            try
            {
                PathPermissions perm = new PathPermissions() { Owner = RolePermissions.Read | RolePermissions.Write };
                await directoryClient.SetPermissionsAsync(perm, userId);

                _logger.LogDebug($"Set Folder permissions for folder: {folderName} user: {userId} SUCCEEDED.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Set Folder permissions for folder: {folderName} user: {userId} FAILED.");
                throw;
            }                
        }
    }
}
