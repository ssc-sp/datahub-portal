using Microsoft.AspNetCore.Components.Forms;
using NRCan.Datahub.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Tewr.Blazor.FileReader;

namespace NRCan.Datahub.Shared.Services
{
    public interface IApiService
    {
        Exception LastException { get; set; }
        Folder CurrentFolder { get; set; }
        Folder MyDataFolder { get; }
        NonHierarchicalFolder SharedDataFolder { get; }
        NonHierarchicalFolder SearchDataFolder { get; }
        string LogoutURL { get; }
        Dictionary<string, FileMetaData> UploadedFiles { get; set; }

        Task LoadApplicationData();
        Task PopulateOtherMetadata(FileMetaData fileMetadata);
        Task UploadFile(FileMetaData fileMetadata);
        Task<long> GetUserUsedDataTotal(Microsoft.Graph.User user);
        Task RestoreVersionOfBlob(string fileid, string versionId);
        Task<Folder> SearchIndex(dynamic folder, string filter, Microsoft.Graph.User user);
        Task<Uri> DownloadFile(FileMetaData file);
        Task<Folder> GetFileList(Folder folder, Microsoft.Graph.User user, bool onlyFolders = false, bool recursive = false);
        Task<bool> DoesFolderExist(string folderName);
        Task UploadGen2File(IBrowserFile browserFile, FileMetaData fileMetadata);
    }    
}