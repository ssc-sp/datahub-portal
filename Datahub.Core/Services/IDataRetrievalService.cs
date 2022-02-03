using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;

namespace Datahub.Core.Services
{
    public interface IDataRetrievalService
    {
        Task<System.Uri> DownloadFile(FileMetaData file, string project);
        Task<List<string>> GetAllFolders(string rootFolderName, User user);
        Task<List<Version>> GetFileVersions(string fileId);
        Task<Data.Folder> GetFolderContents(Data.Folder folder, string filterSearch, User user, string project);
        Task<Data.Folder> GetFolderStructure(Data.Folder folder, User user, bool onlyFolders = true);
        Task<StorageMetadata> GetStorageMetadata(string project);
        
    }
}