using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Shared.Data;
using Datahub.Shared.Services;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineDataRemovalService : IDataRemovalService
    {
        public OfflineDataRemovalService()
        {
        }

        public Task<bool> Delete(Shared.Data.Folder folder, User currentUser)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Delete(FileMetaData file, User currentUser)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DeleteStorageBlob(FileMetaData file, string project, User currentUser)
        {
            return Task.FromResult(true);
        }
    }
}
