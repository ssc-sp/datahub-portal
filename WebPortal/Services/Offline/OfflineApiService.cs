using BlazorInputFile;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tewr.Blazor.FileReader;
using NRCan.Datahub.Shared.Services;
using NRCan.Datahub.Shared.Data;

namespace NRCan.Datahub.Portal.Services.Offline
{

    public class OfflineApiService : IApiService
    {

        private IFileListEntry _file;
        public IFileListEntry GetFile()
        {
            return _file;
        }
        public string CurrentFolderId { get; set; }
        private IUserInformationService _userInformationService;
        private readonly ILogger<OfflineApiService> _logger;
        private List<FileMetaData> _mySharedDataFilelist;

        public Exception LastException { get; set; }

        public Folder CurrentFolder { get; set; }
        public Folder MyDataFolder { get; } = new Folder() {
            id = "-1",
            name = "MyData",
            isShared = false
        };
        public NonHierarchicalFolder SharedDataFolder { get; } = new NonHierarchicalFolder() {
            id = "-2",
            name = "SharedWithYou",
            isShared = true
        };
        public NonHierarchicalFolder SearchDataFolder { get; } = new NonHierarchicalFolder() {
            id = "-3",
            name = "SearchData",
            isShared = false
        };
        public string LogoutURL
        {
            get
            {
                return "";
            }    
        }

        public Dictionary<string, FileMetaData> UploadedFiles { get; set; } = new Dictionary<string, FileMetaData>();

        public OfflineApiService(IUserInformationService userInformationService, ILogger<OfflineApiService> logger)
        {
            _userInformationService = userInformationService;
            _logger = logger;
            MyDataFolder.Add(new FileMetaData()
            {
                createdby = "1",
                createdts = DateTime.Now.Date,
                filename = "OfflineFile.pdf",
                lastmodifiedby = "1",
                lastmodifiedts = DateTime.Now.Date,
                ownedby = "1",
                fileformat = "pdf",
                filesize = "1.8 MB",
                description = "Aenean ligula odio, maximus id posuere porttitor, mollis ut metus. In ligula dui, finibus sit amet ipsum id, ullamcorper tincidunt magna. Suspendisse eu lacus vitae odio efficitur suscipit. Nullam turpis purus, volutpat at ullamcorper id, pulvinar quis massa.",
                tags = new List<string>() { "Decks", "DataHub", "Needs review" },
            });
            MyDataFolder.Add(new FileMetaData()
            {
                createdby = "1",
                createdts = DateTime.Now.Date,
                filename = "OfflineFile2.pdf",
                lastmodifiedby = "1",
                lastmodifiedts = DateTime.Now.Date,
                ownedby = "1",
                fileformat = "pdf",
                filesize = "7.2 MB",
                description = "Donec facilisis lobortis sem quis consectetur. Suspendisse lacus orci, suscipit nec aliquam at, facilisis et erat. Praesent pharetra efficitur elit, at molestie urna commodo id. Nunc volutpat bibendum quam at sagittis. Sed eget aliquam massa. Vivamus ac sagittis sem, et aliquet metus. Donec euismod ullamcorper risus. Phasellus quis orci non purus dignissim ullamcorper.",
                tags = new List<string>() { "To be deleted", "DataHub"},
            });
            MyDataFolder.Sort();
        }

        public async Task DeleteFile(FileMetaData fileMetadata)
        {
            await Task.Run(() =>
            {
                MyDataFolder.Remove(fileMetadata);
                SharedDataFolder.Remove(fileMetadata);
            });
        }

        public async Task PopulateOtherMetadata(FileMetaData fileMetadata)
        {
            var authState = await _userInformationService.GetCurrentUserAsync();
            fileMetadata.createdby = authState.Id;
            fileMetadata.lastmodifiedby = authState.Id; //TODO this will need to change edit functionality
            fileMetadata.ownedby = authState.Id; //TODO this will need to change edit functionality
            fileMetadata.lastmodifiedts = DateTime.Now.Date;
            fileMetadata.fileid = Guid.NewGuid().ToString();
            fileMetadata.fileformat = GetExtension(fileMetadata.filename);
        }

        public async Task UploadFile(FileMetaData fileMetadata)
        {
            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                fileMetadata.uploadStatus = FileUploadStatus.SelectedToUpload;

                UploadedFiles.Add( $"{fileMetadata.folderpath}/{fileMetadata.filename}", fileMetadata);
                   
                fileMetadata.filesize = fileMetadata.fileData.Length.ToString();
                
                fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadSuccess);
                UploadedFiles.Remove( $"{fileMetadata.folderpath}/{fileMetadata.filename}");
                
                _logger.LogInformation("End File Read");
            }
            catch (Exception e)
            {
                fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadError);
                _logger.LogError(e, "Error uploading file");
            }
        }

        public string GetExtension(string FileName)
        {
            int pos = FileName.LastIndexOf(".") + 1;
            return FileName.Substring(pos, FileName.Length - pos);
        }


        public Task<long> GetUserUsedDataTotal(Microsoft.Graph.User user)
        {
            throw new NotImplementedException();
        }


        public Task RestoreVersionOfBlob(string fileid, string versionId)
        {
            throw new NotImplementedException();
        }

        public Task<Folder> SearchIndex(dynamic folder, string filter, Microsoft.Graph.User user)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> DownloadFile(FileMetaData file)
        {
            throw new NotImplementedException();
        }

        public Task<Folder> GetFileList(Folder folder, Microsoft.Graph.User user, bool onlyFolders = false, bool recursive = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesFolderExist(string folderName)
        {
            throw new NotImplementedException();
        }

        public Task LoadApplicationData()
        {
            throw new NotImplementedException();
        }
    }
}
