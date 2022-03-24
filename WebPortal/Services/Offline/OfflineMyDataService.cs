using Microsoft.AspNetCore.Components.Forms;
using Datahub.Core.Data;
using Datahub.Core.Services;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineMyDataService : MyDataService
    {
        private IBrowserFile _file = null;

        public IBrowserFile GetFile()
        {
            return _file;
        }

        public string CurrentFolderId { get; set; }
        private IUserInformationService _userInformationService;
        private readonly ILogger<OfflineMyDataService> _logger;

        public new Exception LastException { get; set; }

        public new Folder CurrentFolder { get; set; }

        public new Folder MyDataFolder { get; } = new Folder() {
            id = "-1",
            name = "MyData",
            isShared = false
        };

        public new NonHierarchicalFolder SharedDataFolder { get; } = new NonHierarchicalFolder() {
            id = "-2",
            name = "SharedWithYou",
            isShared = true
        };

        public new NonHierarchicalFolder SearchDataFolder { get; } = new NonHierarchicalFolder() {
            id = "-3",
            name = "SearchData",
            isShared = false
        };

        public new string LogoutURL => "";

        public new Dictionary<string, FileMetaData> UploadedFiles { get; set; } = new Dictionary<string, FileMetaData>();
        public new IBrowserFile browserFile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ProjectUploadCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public OfflineMyDataService(IUserInformationService userInformationService, ILogger<OfflineMyDataService> logger):
            base(logger,
                    null,
                     userInformationService,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
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

        public new async Task PopulateOtherMetadata(FileMetaData fileMetadata)
        {
            var authState = await _userInformationService.GetUserAsync();
            fileMetadata.createdby = authState.Id;
            fileMetadata.lastmodifiedby = authState.Id; //TODO this will need to change edit functionality
            fileMetadata.ownedby = authState.Id; //TODO this will need to change edit functionality
            fileMetadata.lastmodifiedts = DateTime.Now.Date;
            fileMetadata.fileid = Guid.NewGuid().ToString();
            fileMetadata.fileformat = GetExtension(fileMetadata.filename);
        }
        
        public string GetExtension(string FileName)
        {
            int pos = FileName.LastIndexOf(".") + 1;
            return FileName.Substring(pos, FileName.Length - pos);
        }

        public new Task<long> GetUserUsedDataTotal(Microsoft.Graph.User user)
        {
            return Task.FromResult(0L);
        }

        public new Task RestoreVersionOfBlob(string fileid, string versionId)
        {
            return Task.FromResult(0);
        }

        public new Task<Folder> SearchIndex(dynamic folder, string filter, Microsoft.Graph.User user)
        {
            return Task.FromResult(new Folder());
        }
        public Task<Uri> DownloadFile(FileMetaData file, string projectUploadCode)
        {
            return Task.FromResult(new Uri(""));
        }

        public Task<Folder> GetFileList(Folder folder, Microsoft.Graph.User user, bool onlyFolders = false, bool recursive = false)
        {
            return Task.FromResult(new Folder());

        }

        public new Task<bool> DoesFolderExist(string folderName)
        {
            return Task.FromResult(true);
        }

        public new Task LoadApplicationData()
        {
            return Task.FromResult(0);
        }

        public new Task UploadGen2File(FileMetaData fileMetadata, string projectUploadCode)
        {
            return Task.FromResult(0);
        }

        public Task<Uri> GetUserDelegationSasBlob(string fileName, string project = null, int days = 1)
        {
            return Task.FromResult(new Uri(""));
        }

        public new Task AuditException(Exception ex, string correlationId)
        {
            return Task.FromResult(0);
        }

        public Task<Uri> GetDownloadAccessToSasBlob(string projectUploadCode, string fileName, int daysValidity = 1)
        {
            return Task.FromResult(new Uri(""));
        }

        public Task<Uri> GenerateSasToken(string projectUploadCode, int daysValidity)
        {
            return Task.FromResult(new Uri(""));
        }
    }
}
