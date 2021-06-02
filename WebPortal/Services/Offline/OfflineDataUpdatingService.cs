using System.Threading.Tasks;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.Services;

namespace NRCan.Datahub.Portal.Services.Offline
{
    public class OfflineDataUpdatingService : IDataUpdatingService
    {
        public OfflineDataUpdatingService()
        {
        }

        public Task<bool> MoveFile(FileMetaData file, string newParentFolder, User currentUser)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RenameFile(FileMetaData file, string newFileName, User currentUser)
        {
            return Task.FromResult(true);
        }

        public Task<bool> RenameFolder(Shared.Data.Folder folder, string newFolderName, User currentUser)
        {
            return Task.FromResult(true);
        }
    }
}