using System.Threading.Tasks;
using Datahub.Core.Data;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineDataSharingService : IDataSharingService
    {
        public OfflineDataSharingService()
        {
        }

        public Task<bool> AddSharedUsers(FileMetaData file, string sharedUserId, string role)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangeFileOwner(FileMetaData file, GraphUser newOwner, string currentUserId)
        {
            return Task.FromResult(true);
        }

        public Task LoadSharedUsers(FileMetaData file)
        {
            return Task.FromResult(0);
        }

        public Task<bool> RemoveSharedUsers(FileMetaData file, string sharedUserId)
        {
            return Task.FromResult(true);
        }
    }
}
