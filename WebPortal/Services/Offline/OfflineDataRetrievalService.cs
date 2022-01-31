using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;
using Datahub.Core.Services;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineDataRetrievalService : IDataRetrievalService
    {
        public OfflineDataRetrievalService()
        {
        }

        public Task<System.Uri> DownloadFile(FileMetaData file, string project)
        {
            return Task.FromResult(new System.Uri("anyfile"));
        }

        public Task<List<string>> GetAllFolders(string rootFolderName, User user)
        {
            return Task.FromResult(new List<string>());
        }

        public Task<List<Core.Data.Version>> GetFileVersions(string fileId)
        {
            return Task.FromResult(new List<Core.Data.Version>());
        }

        public Task<Core.Data.Folder> GetFolderContents(Core.Data.Folder folder, string filterSearch, User user, string project)
        {
            return Task.FromResult(new Core.Data.Folder());
        }

        public Task<Core.Data.Folder> GetFolderStructure(Core.Data.Folder folder, User user, bool onlyFolders = true)
        {
            return Task.FromResult(new Core.Data.Folder());
        }

        public Task<StorageMetadata> GetStorageMetadata(string project)
        {
            return Task.FromResult(new StorageMetadata());
        }
    }
}
