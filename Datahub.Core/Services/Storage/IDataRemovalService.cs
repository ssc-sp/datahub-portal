using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public interface IDataRemovalService
    {
        Task<bool> Delete(Data.Folder folder, User currentUser);
        Task<bool> Delete(FileMetaData file, User currentUser);
        Task<bool> DeleteStorageBlob(FileMetaData file, string project, string containerName, User currentUser);
    }
}