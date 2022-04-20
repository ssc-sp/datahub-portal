using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Datahub.Core.Data;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class OfflineDataLakeClientService : IDataLakeClientService
    {
        public async Task<bool> AssignOwnerPermissionsToFile(FileMetaData file, string userId, string permissions)
        {
            return await Task.FromResult(false);
        }

        public async Task<DataLakeFileSystemClient> GetDataLakeFileSystemClient()
        {
            return await Task.FromResult(default(DataLakeFileSystemClient));
        }

        public async Task<DataLakeServiceClient> GetDataLakeServiceClient()
        {
            return await Task.FromResult(default(DataLakeServiceClient));
        }

        public async Task<StorageSharedKeyCredential> GetSharedKeyCredential(string project)
        {
            return await Task.FromResult(default(StorageSharedKeyCredential));
        }

        public async Task<StorageSharedKeyCredential> GetSharedKeyCredential()
        {
            return await Task.FromResult(default(StorageSharedKeyCredential));
        }

        public async Task LoadSharedUsers(FileMetaData file)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> RemoveSharedUser(FileMetaData file, string user)
        {
            return await Task.FromResult(false);
        }
    }
}
