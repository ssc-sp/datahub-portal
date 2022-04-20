using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Datahub.Core.Data;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IDataLakeClientService
    {
        Task<StorageSharedKeyCredential> GetSharedKeyCredential(string project);
        Task<StorageSharedKeyCredential> GetSharedKeyCredential();
        Task<DataLakeServiceClient> GetDataLakeServiceClient();
        Task<DataLakeFileSystemClient> GetDataLakeFileSystemClient();
        Task<bool> AssignOwnerPermissionsToFile(FileMetaData file, string userId, string permissions);
        Task<bool> RemoveSharedUser(FileMetaData file, string user);
        Task LoadSharedUsers(FileMetaData file);
    }
}
