using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using NRCan.Datahub.Shared.Data;

namespace NRCan.Datahub.Shared.Services
{
    public interface IDataRetrievalService
    {
        Task DownloadFile(FileMetaData file);
        Task<List<string>> GetAllFolders(string rootFolderName, User user);
        Task<List<Version>> GetFileVersions(string fileId);
        Task<Data.Folder> GetFolderContents(dynamic folder, string filterSearch, User user);
        Task<Data.Folder> GetFolderStructure(Shared.Data.Folder folder, User user, bool onlyFolders = true);
    }
}