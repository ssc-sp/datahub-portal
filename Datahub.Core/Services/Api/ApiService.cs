using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq;
using Datahub.Core.Data;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace Datahub.Core.Services
{
    public class ApiService : IApiService
    {
        private IOptions<APITarget> _targets;
        private DataLakeClientService _dataLakeClientService;
        private ICognitiveSearchService _cognitiveSearchService;
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;
        private readonly IUserInformationService _userInformationService;
        private readonly IKeyVaultService _keyVaultService;
        private readonly NotifierService _notifierService;
        private readonly IApiCallService _apiCallService;
        private readonly IJSRuntime _jsRuntime;

        private CommonAzureServices _commonAzureServices;
        private ApiTelemetryService _telemetryService;
        private IDatahubAuditingService _auditingService;

        public ApiService(ILogger<ApiService> logger,
                    IHttpClientFactory clientFactory,
                    IUserInformationService userInformationService,
                    IKeyVaultService keyVaultService,
                    IOptions<APITarget> targets,
                    NotifierService notifierService,
                    IApiCallService apiCallService,
                    IJSRuntime jSRuntime,
                    ICognitiveSearchService cognitiveSearchService,
                    DataLakeClientService dataLakeClientService,
                    ApiTelemetryService telemetryService,
                    IDatahubAuditingService auditingService,
                    CommonAzureServices commonAzureServices)
        {
            _logger = logger;
            _httpClient = clientFactory;
            _userInformationService = userInformationService;
            _keyVaultService = keyVaultService;
            _notifierService = notifierService;
            _apiCallService = apiCallService;
            _jsRuntime = jSRuntime;
            _targets = targets;
            _dataLakeClientService = dataLakeClientService;
            _cognitiveSearchService = cognitiveSearchService;
            LastException = null;
            _commonAzureServices = commonAzureServices;
            _telemetryService = telemetryService;
            _auditingService = auditingService;
        }

        public Exception LastException { get; set; }

        private Folder _currentFolder = null;
        public Folder CurrentFolder
        {
            get
            {
                if (this._currentFolder == null)
                {
                    this._currentFolder = this.MyDataFolder;
                }

                return this._currentFolder;
            }
            set
            {
                this._currentFolder = value;
            }
        }
        public Folder MyDataFolder { get; } = new Folder()
        {
            id = "-1",
            name = "MyData",
            isShared = false
        };
        public NonHierarchicalFolder SharedDataFolder { get; } = new NonHierarchicalFolder()
        {
            id = "-2",
            name = "SharedWithYou",
            isShared = true
        };
        public NonHierarchicalFolder SearchDataFolder { get; } = new NonHierarchicalFolder()
        {
            id = "-3",
            name = "SearchData",
            isShared = false
        };
        public string LogoutURL
        {
            get
            {
                return _targets.Value.LogoutURL;
            }
        }

        public IBrowserFile browserFile { get; set; }

        //public string ProjectUploadCode { get; set; }

        public Dictionary<string, FileMetaData> UploadedFiles { get; set; } = new Dictionary<string, FileMetaData>();

        public async Task AuditException(Exception ex, string correlationId)
        {
            LastException = ex;
            await _auditingService.TrackException(ex, (nameof(correlationId), correlationId));
        }

        public async Task LoadApplicationData()
        {
            var rootFolder = await _userInformationService.GetUserRootFolder();
            MyDataFolder.id = rootFolder;
            SharedDataFolder.id = rootFolder;
            SearchDataFolder.id = rootFolder;

            if (CurrentFolder == null)
            {
                CurrentFolder = MyDataFolder;
            }
        }

        public async Task<Folder> SearchIndex(dynamic folder, string filter, Microsoft.Graph.User user)
        {
            try
            {
                var searchIndexClient = await _commonAzureServices.GetSearchClientForIndexing();
                var options = new SearchOptions();
                options.Filter = filter;
                options.IncludeTotalCount = true;

                // Build our list of fields to retrieve from files
                foreach (string propertyName in FileMetaDataExtensions.GetMetadataProperties(null).Where(p => !string.IsNullOrWhiteSpace(p.key) && p.inSearch).Select(p => p.key))
                {
                    options.Select.Add(propertyName);
                }

                folder.Clear();
                SearchResults<FileMetaData> results = searchIndexClient.Search<FileMetaData>("*", options);

                await foreach (Page<SearchResult<FileMetaData>> searchResults in results.GetResultsAsync().AsPages(default, 20))
                {
                    foreach (var item in searchResults.Values)
                    {
                        folder.Add(item.Document, false);
                    }
                }

                folder.Sort();
                _logger.LogDebug($"Searching index: {_targets.Value.FileIndexName} criteria: {filter} user: {user.DisplayName} results: {results.TotalCount.Value} SUCCEEDED.");

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Searching index: {_targets.Value.FileIndexName} criteria: {filter} user: {user.DisplayName} FAILED.");
                throw;
            }
        }

        public async Task<Uri> GetUserDelegationSasBlob(string fileName, string projectUploadCode, int daysValidity = 1)
        {
            return await GetDelegationSasBlobUri(fileName, projectUploadCode, daysValidity, BlobSasPermissions.Read | BlobSasPermissions.Write);
        }

        public async Task<Uri> GetDownloadAccessToSasBlob(string fileName, string projectUploadCode, int daysValidity = 1)
        {
            return await GetDelegationSasBlobUri(fileName, projectUploadCode, daysValidity, BlobSasPermissions.Read);
        }

        public async Task<Uri> GenerateSasToken(string projectUploadCode, int daysValidity)
        {
            return await GetDelegationSasBlobUri(null, projectUploadCode, daysValidity, BlobSasPermissions.All);
        }

        public async Task<Uri> DownloadFile(FileMetaData file, string? projectUploadCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(projectUploadCode))
                {
                    return await GetUserDelegationSasBlob(file.filename, projectUploadCode);
                }

                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(file.folderpath);
                DataLakeFileClient fileClient = directoryClient.GetFileClient(file.filename);
                Response<FileDownloadInfo> downloadResponse = await fileClient.ReadAsync();

                var sharedKeyCredential = await _dataLakeClientService.GetSharedKeyCredential();


                DataLakeSasBuilder sasBuilder = new DataLakeSasBuilder()
                {
                    FileSystemName = fileSystemClient.Name,
                    Path = fileClient.Path,
                    Resource = "d",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddDays(7)
                };

                // Specify read and write permissions for the SAS.
                sasBuilder.SetPermissions(DataLakeAccountSasPermissions.Read |
                                          DataLakeAccountSasPermissions.Write);


                DataLakeUriBuilder dataLakeUriBuilder = new DataLakeUriBuilder(fileClient.Uri)
                {
                    // Specify the user delegation key.
                    //Sas = sasBuilder.ToSasQueryParameters(userDelegationKey,
                    //                                  fileClient.AccountName)

                    Sas = sasBuilder.ToSasQueryParameters(sharedKeyCredential)
                };

                _logger.LogDebug($"File URI Generation: {file.folderpath}/{file.filename} SUCCEEDED.");

                return dataLakeUriBuilder.ToUri();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"File URI Generation: {file.folderpath}/{file.filename} FAILED.");
                throw;
            }
        }

        public async Task UploadGen2File(FileMetaData fileMetadata, string? projectUploadCode)
        {
            //await _notifierService.Update($"{fileMetadata.folderpath}/{fileMetadata.filename}", true);
            try
            {
                fileMetadata.bytesToUpload = browserFile.Size;
                fileMetadata.filesize = browserFile.Size.ToString();
                fileMetadata.folderpath = string.IsNullOrEmpty(projectUploadCode) ? fileMetadata.folderpath : string.Empty;


                if (string.IsNullOrEmpty(projectUploadCode))
                {
                    await UploadToGen2Storage(fileMetadata);
                    await _cognitiveSearchService.AddDocumentToIndex(fileMetadata);
                }
                else
                {
                    //await UploadToProject(fileMetadata);
                    await UploadFileToProject(fileMetadata, projectUploadCode);
                }

                fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadSuccess);

                // Done, so we can remove!
                UploadedFiles.Remove($"{fileMetadata.folderpath}/{fileMetadata.filename}");

                _logger.LogInformation($"Uploading file: {fileMetadata.folderpath}/{fileMetadata.filename} User: {fileMetadata.ownedby}");
            }
            catch (Exception e)
            {
                fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadError);
                _logger.LogError(e, $"Error uploading file: {fileMetadata.folderpath}/{fileMetadata.filename} User: {fileMetadata.ownedby}");

            }

            // Done upload, update ONE last time!
            await _notifierService.Update($"{fileMetadata.folderpath}/{fileMetadata.filename}", true);


        }

        private async Task UploadToProject(FileMetaData fileMetadata, string projectUploadCode)
        {
            string cxnstring = await _apiCallService.GetProjectConnectionString(projectUploadCode);
            BlobServiceClient blobServiceClient = new BlobServiceClient(cxnstring);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("datahub");


            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = containerClient.GetBlobClient(fileMetadata.filename);

            var metadata = fileMetadata.GenerateMetadata();
            var uploadOptions = new BlobUploadOptions()
            {
                ProgressHandler = new Progress<long>((progress) =>
                {
                    fileMetadata.uploadedBytes = progress;
                    _ = _notifierService.Update($"adddata", false);
                })
            };

            long maxFileSize = 1024000000000;
            await blob.UploadAsync(browserFile.OpenReadStream(maxFileSize), uploadOptions);
            await blob.SetMetadataAsync(metadata);

            // Upload local file

        }

        private async Task UploadFileToProject(FileMetaData fileMetadata, string projectUploadCode)
        {
            string cxnstring = await _apiCallService.GetProjectConnectionString(projectUploadCode);
            long maxFileSize = 1024000000000;
            var metadata = fileMetadata.GenerateMetadata();

            var blobClientUtil = new Utils.BlobClientUtils(cxnstring, "datahub");

            await blobClientUtil.UploadFile(fileMetadata.filename, browserFile.OpenReadStream(maxFileSize), metadata, (progress) =>
            {
                fileMetadata.uploadedBytes = progress;
                _ = _notifierService.Update($"adddata", false);
            });
        }

        private async Task UploadToGen2Storage(FileMetaData fileMetadata)
        {
            try
            {

                fileMetadata.uploadStatus = FileUploadStatus.UploadingToRepository;

                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(fileMetadata.folderpath);
                DataLakeFileClient fileClient = directoryClient.GetFileClient(fileMetadata.filename);


                //await fileClient.UploadAsync()

                var fileUploading = $"{fileMetadata.folderpath}/{fileMetadata.filename}";

                var uploadOptions = new DataLakeFileUploadOptions()
                {
                    ProgressHandler = new Progress<long>((progress) =>
                    {
                        fileMetadata.uploadedBytes = progress;
                        _ = _notifierService.Update($"adddata", false);
                    })
                };


                Stopwatch watch = new Stopwatch();
                watch.Start();

                long maxFileSize = 1024000000000;

                await fileClient.UploadAsync(browserFile.OpenReadStream(maxFileSize), uploadOptions);

                watch.Stop();
                _telemetryService.LogFileUploadSize(fileMetadata.uploadedBytes, fileUploading);
                _telemetryService.LogFileUploadTime(watch.ElapsedMilliseconds, fileUploading);
                _telemetryService.LogFileUploadBpms(fileMetadata.uploadedBytes / watch.ElapsedMilliseconds, fileUploading);

                var metadata = fileMetadata.GenerateMetadata();
                //metadata.Add("IsDeleted", "0");
                fileClient.SetMetadata(metadata);

                PathPermissions perm = new PathPermissions()
                {
                    Owner = RolePermissions.Read | RolePermissions.Write
                };

                await fileClient.SetPermissionsAsync(perm, fileMetadata.ownedby);

                fileMetadata.uploadStatus = FileUploadStatus.FileUploadSuccess;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading to Gen2 storage");
                throw;
            }
        }

        public async Task PopulateOtherMetadata(FileMetaData fileMetadata)
        {
            var authState = await _userInformationService.GetUserAsync();

            fileMetadata.createdby = authState.Id;
            fileMetadata.lastmodifiedby = authState.Id; //TODO this will need to change edit functionality
            fileMetadata.ownedby = authState.Id; //TODO this will need to change edit functionality
            fileMetadata.lastmodifiedts = DateTime.Now.Date;

            if (string.IsNullOrWhiteSpace(fileMetadata.fileid))
            {
                fileMetadata.fileid = Guid.NewGuid().ToString();
            }
        }

        public async Task RestoreVersionOfBlob(string versionId, string fileid)
        {

            try
            {
                // Get the name of the first blob in the container to use as the source.
                BlobServiceClient blobServiceClient = new BlobServiceClient(await _apiCallService.getStorageConnString());
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_apiCallService.getBlobContainerName());
                BlobClient sourceBlob = containerClient.GetBlobClient(fileid).WithVersion(versionId);

                // Create a BlobClient representing the source blob to copy.

                // Ensure that the source blob exists.
                if (await sourceBlob.ExistsAsync())
                {
                    // Lease the source blob for the copy operation 
                    // to prevent another client from modifying it.
                    BlobLeaseClient lease = sourceBlob.GetBlobLeaseClient();

                    // Specifying -1 for the lease interval creates an infinite lease.
                    //await lease.AcquireAsync(TimeSpan.FromSeconds(60));

                    // Get the source blob's properties and display the lease state.
                    Azure.Storage.Blobs.Models.BlobProperties sourceProperties = await sourceBlob.GetPropertiesAsync();
                    //Console.WriteLine($"Lease state: {sourceProperties.LeaseState}");

                    // Get a BlobClient representing the destination blob with a unique name.
                    BlobClient destBlob =
                        containerClient.GetBlobClient(fileid);


                    await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                    // Get the destination blob's properties and display the copy status.
                    Azure.Storage.Blobs.Models.BlobProperties destProperties = await destBlob.GetPropertiesAsync();

                    //Console.WriteLine($"Copy status: {destProperties.CopyStatus}");
                    //Console.WriteLine($"Copy progress: {destProperties.CopyProgress}");
                    //Console.WriteLine($"Completion time: {destProperties.CopyCompletedOn}");
                    //Console.WriteLine($"Total bytes: {destProperties.ContentLength}");

                    // Update the source blob's properties.
                    sourceProperties = await sourceBlob.GetPropertiesAsync();

                    if (sourceProperties.LeaseState == Azure.Storage.Blobs.Models.LeaseState.Leased)
                    {
                        // Break the lease on the source blob.
                        await lease.BreakAsync();

                        // Update the source blob's properties to check the lease state.
                        sourceProperties = await sourceBlob.GetPropertiesAsync();
                        //Console.WriteLine($"Lease state: {sourceProperties.LeaseState}");
                    }
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Error uploading file to blob");
            }
        }

        public async Task<long> GetUserUsedDataTotal(Microsoft.Graph.User user)
        {
            

            Folder rootFolder = new Folder()
            {
                id = MyDataFolder.id,
                name = MyDataFolder.name,
                isShared = false
            };
            //TODO - refactor this call. GetFileList has been moved to the Data Retrieval Service
            //rootFolder = await GetFileList(rootFolder, user, false, true);

            return await Task.FromResult(rootFolder.TotalSpace());
        }

        public async Task<bool> DoesFolderExist(string folderName)
        {
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(folderName);

            return directoryClient.Exists();
        }

        const string defaultContainerName = "datahub";

        static BlobSasBuilder GetBlobSasBuilder(string fileName, int days, BlobSasPermissions permissions)
        {
            var result = new BlobSasBuilder()
            {
                BlobContainerName = defaultContainerName,
                BlobName = fileName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(days)
            };

            result.SetPermissions(permissions);

            return result;
        }

        private async Task<BlobContainerClient> GetBlobContainerClient(string project)
        {
            var cxnstring = await _apiCallService.GetProjectConnectionString(project);
            BlobServiceClient blobServiceClient = new BlobServiceClient(cxnstring);
            return blobServiceClient.GetBlobContainerClient(defaultContainerName);
        }

        private async Task<Uri> GetDelegationSasBlobUri(string fileName, string projectUploadCode, int days, BlobSasPermissions permissions)
        {
            var project = projectUploadCode.ToLowerInvariant();
            var containerClient = await GetBlobContainerClient(project);
            var sasBuilder = GetBlobSasBuilder(fileName, days, permissions);

            var sharedKeyCred = await _dataLakeClientService.GetSharedKeyCredential(project);

            Uri uri;
            if (!string.IsNullOrEmpty(fileName))
            {
                var blobClient = containerClient.GetBlobClient(fileName);
                uri = blobClient.Uri;
            }
            else
            {
                uri = containerClient.Uri;
            }

            var blobUriBuilder = new BlobUriBuilder(uri)
            {
                Sas = sasBuilder.ToSasQueryParameters(sharedKeyCred)
            };

            return blobUriBuilder.ToUri();
        }
    }
}