using System.Threading.Tasks;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Services;

namespace NRCan.Datahub.Portal.Services
{
    public class OfflineDataCreatorService : IDataCreatorService
    {
        public OfflineDataCreatorService()
        {
        }

        public Task<bool> CreateFolder(Shared.Data.Folder folder, Shared.Data.Folder parent, User user)
        {
            return Task.FromResult(true);
        }

        public Task<bool> CreateRootFolderIfNotExist(string userId, string rootFolder)
        {
            return Task.FromResult(true);
        }
    }
}
