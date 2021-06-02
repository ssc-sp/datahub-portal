using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using NRCan.Datahub.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public class DataLakeClientService
    {
        private CognitiveSearchService _cognitiveSearchService;
        private ILogger<DataLakeClientService> _logger;
        private IKeyVaultService _keyVaultService;
        private IOptions<APITarget> _targets;
        private StorageSharedKeyCredential _sharedKeyCredential;

        public DataLakeClientService(ILogger<DataLakeClientService> logger,
                    IKeyVaultService keyVaultService,
                    IOptions<APITarget> targets,
                    CognitiveSearchService cognitiveSearchService
                    )
        {
            _cognitiveSearchService = cognitiveSearchService;
            _logger = logger;
            _keyVaultService = keyVaultService;
            _targets = targets;            
        }

        private DataLakeServiceClient dataLakeServiceClient { get; set; }
        private DataLakeFileSystemClient dataLakeFileSystemClient { get; set; }
        private async Task SetDataLakeServiceClient()
        {
            var datalakeSecret = await _keyVaultService.GetSecret("Datahub-StorageDL-Secret");
            _sharedKeyCredential = new StorageSharedKeyCredential(_targets.Value.StorageAccountName, datalakeSecret);
            string dfsUri = $"https://{_targets.Value.StorageAccountName}.dfs.core.windows.net";

            dataLakeServiceClient = new DataLakeServiceClient(new Uri(dfsUri), _sharedKeyCredential);
            dataLakeFileSystemClient = dataLakeServiceClient.GetFileSystemClient(_targets.Value.FileSystemName);
        }


        public async Task<StorageSharedKeyCredential> GetSharedKeyCredential()
        {
            await CheckClients();
            return _sharedKeyCredential;
        }

        public async Task<DataLakeServiceClient> GetDataLakeServiceClient()
        {
            await CheckClients();
            return dataLakeServiceClient;
        }

        public async Task<DataLakeFileSystemClient> GetDataLakeFileSystemClient()
        {
            await CheckClients();
            return dataLakeFileSystemClient;
        }

        public async Task<bool> AssignOwnerPermissionsToFile(FileMetaData file, string userId, string permissions)
        {            
            var accessControlTuple = await GetAccessControlList(file);
            var accessControlList = accessControlTuple.Item1;
            var fileClient = accessControlTuple.Item2;

            IList<PathAccessControlItem> listItem = PathAccessControlExtensions.ParseAccessControlList($"user:{userId}:{permissions}"); //rwx

            // 1) Check to see if user is already in the list
            var newPerm = listItem.First();
            var userPerm = accessControlList.FirstOrDefault(p => p.EntityId == newPerm.EntityId);
            if (userPerm != null)
            {
                userPerm.Permissions = newPerm.Permissions;
            }
            else
            {
                accessControlList.Add(newPerm);
            }
            var response = fileClient.SetAccessControlList(accessControlList);
            if (response.GetRawResponse().Status == 200)
            {
                await LoadSharedUsers(file);

                file.lastmodifiedts = DateTime.UtcNow;
                fileClient.SetMetadata(file.GenerateMetadata());
                await _cognitiveSearchService.EditDocument(file);

                return true;
            }

            return false;
        }

        public async Task<bool> RemoveSharedUser(FileMetaData file, string user)
        {
            var accessControlTuple = await GetAccessControlList(file);
            var accessControlList = accessControlTuple.Item1;
            var fileClient = accessControlTuple.Item2;
            var item = accessControlList.Where(a => a.EntityId == user).FirstOrDefault();
            if (item != null)
            {
                accessControlList.Remove(item);
            }

            var response = fileClient.SetAccessControlList(accessControlList);

            await LoadSharedUsers(file);

            file.lastmodifiedts = DateTime.UtcNow;
            fileClient.SetMetadata(file.GenerateMetadata());
            await _cognitiveSearchService.EditDocument(file);

            return response.GetRawResponse().Status == 200;
        }

        public async Task LoadSharedUsers(FileMetaData file)
        {
            var accessControlTuple = await GetAccessControlList(file);
            var accessControlList = accessControlTuple.Item1;

            file.sharedwith.Clear();

            foreach (var item in accessControlList.Where(i => i.AccessControlType == AccessControlType.User && !string.IsNullOrEmpty(i.EntityId) && i.EntityId != file.ownedby))
            {
                Sharedwith sharedwith = new Sharedwith();
                sharedwith.userid = item.EntityId;
                sharedwith.role = item.Permissions.HasFlag(RolePermissions.Write) ? "Editor" : "Viewer";
                file.sharedwith.Add(sharedwith);                
            }
        }

        private async Task<(List<PathAccessControlItem>, DataLakeFileClient)> GetAccessControlList(FileMetaData fileMetadata)
        {
            await CheckClients();

            DataLakeDirectoryClient directoryClient = dataLakeFileSystemClient.GetDirectoryClient(fileMetadata.folderpath);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(fileMetadata.filename);
            PathAccessControl fileAccessControl = await fileClient.GetAccessControlAsync();

            return (fileAccessControl.AccessControlList.ToList(), fileClient);
        }

        private async Task CheckClients()
        {
            if (dataLakeFileSystemClient == null)
            {
                await SetDataLakeServiceClient();
            }
        }
    }
}
