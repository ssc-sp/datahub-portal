using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading;
using Tewr.Blazor.FileReader;
using NRCan.Datahub.Shared.Data;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.JSInterop;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.ApplicationInsights;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Diagnostics;
using Azure.Storage.Sas;
using Azure.Storage;

namespace NRCan.Datahub.Shared.Services
{
    public class ApiService : IApiService
    {
        private IOptions<APITarget> _targets;
        private DataLakeClientService _dataLakeClientService;
        private CognitiveSearchService _cognitiveSearchService;
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;
        private readonly IUserInformationService _userInformationService;
        private readonly IKeyVaultService _keyVaultService;
        private readonly NotifierService _notifierService;
        private readonly ApiCallService _apiCallService;
        private readonly IJSRuntime _jsRuntime;

        private CommonAzureServices _commonAzureServices;
        private ApiTelemetryService _telemetryService;
        public ApiService(ILogger<ApiService> logger,
                    IHttpClientFactory clientFactory,
                    IUserInformationService userInformationService,
                    IKeyVaultService keyVaultService,
                    IOptions<APITarget> targets,
                    NotifierService notifierService,
                    ApiCallService apiCallService,
                    IJSRuntime jSRuntime,
                    CognitiveSearchService cognitiveSearchService,
                    DataLakeClientService dataLakeClientService,
                    ApiTelemetryService telemetryService,
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
                return _targets.Value.LogoutURL;
            }    
        }

        public Dictionary<string, FileMetaData> UploadedFiles { get; set; } = new Dictionary<string, FileMetaData>();

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
                foreach (string propertyName in FileMetaDataExtensions.GetMetadataProperties(null).Where(p => !string.IsNullOrWhiteSpace(p.key) && p.inSearch).Select( p => p.key))
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

        public async Task<Uri> DownloadFile(FileMetaData file)
        {
            try
            {
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

       

        public async Task UploadFile(FileMetaData fileMetadata)
        {
            try
            {
                fileMetadata.bytesToUpload = fileMetadata.fileData.Length;
                fileMetadata.filesize = fileMetadata.bytesToUpload.ToString();                                

                // Ok, so we need to upload to gen 2 storage!
                await UploadToGen2Storage(fileMetadata, fileMetadata.fileData);

                //await _cognitiveSearchService.RunIndexerAsnyc();
                await _cognitiveSearchService.AddDocumentToIndex(fileMetadata);
                
                fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadSuccess);

                // Done, so we can remove!
                UploadedFiles.Remove( $"{fileMetadata.folderpath}/{fileMetadata.filename}");

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

        private async Task UploadToGen2Storage(FileMetaData fileMetadata, Stream ms)
        {
            try
            {
                fileMetadata.uploadStatus = FileUploadStatus.UploadingToRepository;

                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(fileMetadata.folderpath);
                DataLakeFileClient fileClient = directoryClient.GetFileClient(fileMetadata.filename);

                var fileUploading = $"{fileMetadata.folderpath}/{fileMetadata.filename}";

                var uploadOptions = new DataLakeFileUploadOptions()
                {
                    ProgressHandler = new Progress<long>((progress) =>
                    {
                        fileMetadata.uploadedBytes = progress;
                        _notifierService.Update($"{fileMetadata.folderpath}/{fileMetadata.filename}", true);
                    })
                };

                ms.Position = 0;

                Stopwatch watch = new Stopwatch();
                watch.Start();

                await fileClient.UploadAsync(ms, uploadOptions);

                watch.Stop();
                _telemetryService.LogFileUploadSize(fileMetadata.uploadedBytes, fileUploading);
                _telemetryService.LogFileUploadTime(watch.ElapsedMilliseconds, fileUploading);
                _telemetryService.LogFileUploadBpms(fileMetadata.uploadedBytes/watch.ElapsedMilliseconds, fileUploading);
                
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
            var authState = await _userInformationService.GetCurrentUserAsync();

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

        protected BaseMetadata Map(PathItem item, DataLakeDirectoryClient directoryClient)
        {
            var itemName = System.IO.Path.GetFileName(item.Name);
            if (item.IsDirectory == true)
            {
                return new Folder()
                {
                    name = itemName,
                    id = itemName,
                    createdby = item.Owner,
                    lastmodifiedby = item.Owner,
                    lastmodifiedts = item.LastModified.DateTime
                };
            }

            DataLakeFileClient fileClient = directoryClient.GetFileClient(itemName);
            PathProperties properties = fileClient.GetProperties();
            var file = new FileMetaData()
            {
                filename = itemName,
                ownedby = item.Owner,
                createdby = item.Owner,
                lastmodifiedby = item.Owner,
                lastmodifiedts = item.LastModified.DateTime
            };

            file.ParseDictionary(properties.Metadata);

            return file;
        }

        public async Task<Folder> GetFileList(Folder folder, Microsoft.Graph.User user, bool onlyFolders = false, bool recursive = false)
        {
            try
            {
                var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
                var directoryClient = fileSystemClient.GetDirectoryClient(folder.fullPathFromRoot);
                var subdirectories = directoryClient.GetPathsAsync().AsPages(default, 20);

                await foreach (Azure.Page<PathItem> directoryPage in subdirectories)
                {
                    // The directoryPage.Values will contain both files and folders
                    // We will ALWAYS add subfolders
                    // ONLY add files IFF onlyFolders is false!
                    foreach (var item in directoryPage.Values.Where(i => i.IsDirectory.Value || !onlyFolders ))
                    {
                        dynamic child = Map(item, directoryClient);
                        folder.Add(child, false);

                        // If directrory and recursive, go down the tree
                        // THIS IS USUALLY ONLY FOR Folder lists!
                        if (item.IsDirectory.Value && recursive)
                        {
                            child = await GetFileList(child, user, onlyFolders, recursive);
                        }
                    }
                }

                folder.Sort();
                _logger.LogDebug($"Get file list for folder: {folder.fullPathFromRoot} for user: {user.DisplayName} results: {folder.children.Count} SUCCEEDED.");

                return folder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get file list for folder: {folder.fullPathFromRoot} for user: {user.DisplayName} FAILED.");
                throw;
            }
        }

        public async Task<long> GetUserUsedDataTotal(Microsoft.Graph.User user)
        {
            Folder rootFolder = new Folder() {  id = MyDataFolder.id,
                                                name = MyDataFolder.name,
                                                isShared = false
                                            };
            rootFolder = await GetFileList(rootFolder, user, false, true);

            return rootFolder.TotalSpace();
        }

        public async Task<bool> DoesFolderExist(string folderName)
        {
            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(folderName);
            
            return directoryClient.Exists();
        }
    }
}