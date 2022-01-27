using Microsoft.AspNetCore.Components.Forms;
using Datahub.Core.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Tewr.Blazor.FileReader;

namespace Datahub.Core.Services
{
    public interface IApiService
    {
        IBrowserFile browserFile { get; set; }
        Exception LastException { get; set; }
        Folder CurrentFolder { get; set; }
        Folder MyDataFolder { get; }
        NonHierarchicalFolder SharedDataFolder { get; }
        NonHierarchicalFolder SearchDataFolder { get; }
        string LogoutURL { get; }
        Dictionary<string, FileMetaData> UploadedFiles { get; set; }

        Task AuditException(Exception ex, string correlationId);
        Task LoadApplicationData();
        Task PopulateOtherMetadata(FileMetaData fileMetadata);        
        Task<long> GetUserUsedDataTotal(Microsoft.Graph.User user);
        Task<bool> DoesFolderExist(string folderName);
        Task RestoreVersionOfBlob(string fileid, string versionId);
        Task<Folder> SearchIndex(dynamic folder, string filter, Microsoft.Graph.User user);
        Task<Uri> DownloadFile(FileMetaData file, string? projectUploadCode);
        Task UploadGen2File(FileMetaData fileMetadata, string? projectUploadCode);

        Task<Uri> GetUserDelegationSasBlob(FileMetaData? file, string projectUploadCode, int daysValidity = 1);
    }    
}