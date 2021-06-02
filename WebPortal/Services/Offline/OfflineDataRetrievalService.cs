using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;
using NRCan.Datahub.Shared.Services;

namespace NRCan.Datahub.Portal.Services.Offline
{
    public class OfflineDataRetrievalService : IDataRetrievalService
    {
        public OfflineDataRetrievalService()
        {
        }

        public Task DownloadFile(FileMetaData file)
        {
            return Task.FromResult(0);
        }

        public Task<List<string>> GetAllFolders(string rootFolderName, User user)
        {
            return Task.FromResult(new List<string>());
        }

        public Task<List<Shared.Data.Version>> GetFileVersions(string fileId)
        {
            return Task.FromResult(new List<Shared.Data.Version>());
        }

        public Task<Shared.Data.Folder> GetFolderContents(dynamic folder, string filterSearch, User user)
        {
            return Task.FromResult(new Shared.Data.Folder());
        }

        public Task<Shared.Data.Folder> GetFolderStructure(Shared.Data.Folder folder, User user, bool onlyFolders = true)
        {
            return Task.FromResult(new Shared.Data.Folder());
        }
    }
}
