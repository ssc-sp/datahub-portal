using Datahub.Core.Data;
using Microsoft.Graph;
using Folder = Datahub.Core.Data.Folder;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;

namespace Datahub.Core.Services
{
    public interface IMyDataService
    {
        Exception LastException { get; set; }
        Folder CurrentFolder { get; set; }
        Folder MyDataFolder { get; }
        NonHierarchicalFolder SharedDataFolder { get; }
        NonHierarchicalFolder SearchDataFolder { get; }
        string LogoutURL { get; }
        IBrowserFile browserFile { get; set; }
        public Dictionary<string, FileMetaData> UploadedFiles { get; set; }
        Task AuditException(Exception ex, string correlationId);
        Task SetupUserFolders();
        Task<Folder> SearchIndex(dynamic folder, string filter, User user);
        Task UploadGen2File(FileMetaData fileMetadata, string projectUploadCode, string containerName);
        Task UploadGen2File(FileMetaData fileMetadata, string projectUploadCode, string containerName, Action<long> progress);
        Task PopulateOtherMetadata(FileMetaData fileMetadata);
        Task<Folder> GetFolderStructure(Folder folder, User user, bool onlyFolders = true);
        Task<Folder> GetFolderContents(Folder folder, string filterSearch, User user);
        Task RestoreVersionOfBlob(string fileid, string versionId);
        Task<long> GetUserUsedDataTotal(User user);
        Task<bool> DoesFolderExist(string folderName);
    }
}
