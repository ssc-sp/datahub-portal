using System.Diagnostics;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Services;
using Datahub.Core.Services.Api;
using Datahub.Infrastructure.Services.Api;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using Microsoft.JSInterop;
using Folder = Datahub.Core.Data.Folder;

namespace Datahub.Infrastructure.Services.Storage;

/// <summary>
/// Important note - MyDataService is not used but the code is here because 
/// this service has all the logic to work with ACLs
/// </summary>
public class MyDataService
{
    private IOptions<APITarget> _targets;
    private DataLakeClientService _dataLakeClientService;
    private readonly IHttpClientFactory _httpClient;
    private readonly ILogger _logger;
    private readonly IUserInformationService _userInformationService;
    private readonly IKeyVaultService _keyVaultService;
    private readonly NotifierService _notifierService;
    private readonly IJSRuntime _jsRuntime;
    private readonly DataRetrievalService dataRetrievalService;
    private CommonAzureServices _commonAzureServices;
    private ApiTelemetryService _telemetryService;
    private IDatahubAuditingService _auditingService;

    public MyDataService(ILogger<MyDataService> logger,
        IHttpClientFactory clientFactory,
        IUserInformationService userInformationService,
        IKeyVaultService keyVaultService,
        IOptions<APITarget> targets,
        NotifierService notifierService,
        IJSRuntime jSRuntime,
        DataRetrievalService dataRetrievalService,
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
        _jsRuntime = jSRuntime;
        this.dataRetrievalService = dataRetrievalService;
        _targets = targets;
        _dataLakeClientService = dataLakeClientService;
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
        Id = "-1",
        Name = "MyData",
        IsShared = false
    };
    public NonHierarchicalFolder SharedDataFolder { get; } = new NonHierarchicalFolder()
    {
        Id = "-2",
        Name = "SharedWithYou",
        IsShared = true
    };
    public NonHierarchicalFolder SearchDataFolder { get; } = new NonHierarchicalFolder()
    {
        Id = "-3",
        Name = "SearchData",
        IsShared = false
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

    public async Task SetupUserFolders()
    {
        var rootFolder = await _userInformationService.GetUserRootFolder();
        MyDataFolder.Id = rootFolder;
        SharedDataFolder.Id = rootFolder;
        SearchDataFolder.Id = rootFolder;

        if (CurrentFolder == null)
        {
            CurrentFolder = MyDataFolder;
        }
    }

    public async Task<Folder> SearchIndex(dynamic folder, string filter, User user)
    {
        try
        {
            var searchIndexClient = await _commonAzureServices.GetSearchClientForIndexing();
            var options = new SearchOptions();
            options.Filter = filter;
            options.IncludeTotalCount = true;

            // Build our list of fields to retrieve from files
            foreach (string propertyName in FileMetaDataExtensions.GetMetadataProperties(null).Where(p => !string.IsNullOrWhiteSpace(p.Key) && p.InSearch).Select(p => p.Key))
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

    public async Task UploadGen2File(FileMetaData fileMetadata, string projectUploadCode, string containerName)
    {
        await UploadGen2File(fileMetadata, projectUploadCode, containerName, (progress) =>
        {
            fileMetadata.UploadedBytes = progress;
            _ = _notifierService.Update($"adddata", false);
        });
    }
    public async Task UploadGen2File(FileMetaData fileMetadata, string projectUploadCode, string containerName, Action<long> progress)
    {
        //await _notifierService.Update($"{fileMetadata.folderpath}/{fileMetadata.filename}", true);
        try
        {
            if (fileMetadata.BrowserFile != null)
            {
                fileMetadata.BytesToUpload = fileMetadata.BrowserFile.Size;
                fileMetadata.Filesize = fileMetadata.BrowserFile.Size.ToString();
            }
            else
            {
                fileMetadata.BytesToUpload = browserFile.Size;
                fileMetadata.Filesize = browserFile.Size.ToString();
            }

            fileMetadata.Folderpath = string.IsNullOrEmpty(projectUploadCode) ? fileMetadata.Folderpath : string.Empty;


            if (string.IsNullOrEmpty(projectUploadCode))
            {
                await UploadToGen2Storage(fileMetadata);
                //await _cognitiveSearchService.AddDocumentToIndex(fileMetadata);
            }
            else
            {
                //await UploadToProject(fileMetadata);
                await UploadFileToProject(fileMetadata, projectUploadCode, containerName, progress);
            }

            fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadSuccess);

            // Done, so we can remove!
            UploadedFiles.Remove($"{fileMetadata.Folderpath}/{fileMetadata.Filename}");

            _logger.LogInformation($"Uploading file: {fileMetadata.Folderpath}/{fileMetadata.Filename} User: {fileMetadata.Ownedby}");
        }
        catch (Exception e)
        {
            fileMetadata.FinishUploadInfo(FileUploadStatus.FileUploadError);
            _logger.LogError(e, $"Error uploading file: {fileMetadata.Folderpath}/{fileMetadata.Filename} User: {fileMetadata.Ownedby}");

        }

        // Done upload, update ONE last time!
        await _notifierService.Update($"{fileMetadata.Folderpath}/{fileMetadata.Filename}", true);


    }

    private async Task UploadFileToProject(FileMetaData fileMetadata, string projectUploadCode, string containerName, Action<long> progress)
    {
        string cxnstring = await dataRetrievalService.GetProjectConnectionString(projectUploadCode);
        long maxFileSize = 1024000000000;
        var metadata = fileMetadata!.GenerateMetadata();

        var blobClientUtil = new Core.Utils.BlobClientUtils(cxnstring, containerName);

        await using var stream = fileMetadata.BrowserFile?.OpenReadStream(maxFileSize) ??
                                 browserFile?.OpenReadStream(maxFileSize);

        await blobClientUtil.UploadFile(fileMetadata.Filename, stream, metadata, progress);
    }

    private async Task UploadToGen2Storage(FileMetaData fileMetadata)
    {
        try
        {

            fileMetadata.UploadStatus = FileUploadStatus.UploadingToRepository;

            var fileSystemClient = await _dataLakeClientService.GetDataLakeFileSystemClient();
            var directoryClient = fileSystemClient.GetDirectoryClient(fileMetadata.Folderpath);
            DataLakeFileClient fileClient = directoryClient.GetFileClient(fileMetadata.Filename);


            //await fileClient.UploadAsync()

            var fileUploading = $"{fileMetadata.Folderpath}/{fileMetadata.Filename}";

            var uploadOptions = new DataLakeFileUploadOptions()
            {
                ProgressHandler = new Progress<long>((progress) =>
                {
                    fileMetadata.UploadedBytes = progress;
                    _ = _notifierService.Update($"adddata", false);
                })
            };


            Stopwatch watch = new Stopwatch();
            watch.Start();

            long maxFileSize = 1024000000000;

            await fileClient.UploadAsync(browserFile.OpenReadStream(maxFileSize), uploadOptions);

            watch.Stop();
            _telemetryService.LogFileUploadSize(fileMetadata.UploadedBytes, fileUploading);
            _telemetryService.LogFileUploadTime(watch.ElapsedMilliseconds, fileUploading);
            _telemetryService.LogFileUploadBpms(fileMetadata.UploadedBytes / watch.ElapsedMilliseconds, fileUploading);

            var metadata = fileMetadata.GenerateMetadata();
            //metadata.Add("IsDeleted", "0");
            fileClient.SetMetadata(metadata);

            PathPermissions perm = new PathPermissions()
            {
                Owner = RolePermissions.Read | RolePermissions.Write
            };

            await fileClient.SetPermissionsAsync(perm, fileMetadata.Ownedby);

            fileMetadata.UploadStatus = FileUploadStatus.FileUploadSuccess;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error uploading to Gen2 storage");
            throw;
        }
    }

    public async Task PopulateOtherMetadata(FileMetaData fileMetadata)
    {
        var authState = await _userInformationService.GetCurrentGraphUserAsync();

        fileMetadata.Createdby = authState.Id;
        fileMetadata.Lastmodifiedby = authState.Id; //TODO this will need to change edit functionality
        fileMetadata.Ownedby = authState.Id; //TODO this will need to change edit functionality
        fileMetadata.Lastmodifiedts = DateTime.Now.Date;

        if (string.IsNullOrWhiteSpace(fileMetadata.Fileid))
        {
            fileMetadata.Fileid = Guid.NewGuid().ToString();
        }
    }

    public async Task<Folder> GetFolderStructure(Folder folder, User user, bool onlyFolders = true)
    {
        try
        {
            return await dataRetrievalService.GetFileList(folder, user, onlyFolders, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFolderStructure folder: {FullPathFromRoot} user: {DisplayName} FAILED", folder.FullPathFromRoot, user.DisplayName);
            throw;
        }
    }

    public async Task<Folder> GetFolderContents(Folder folder, string filterSearch, User user)
    {
        try
        {
            // Clear folder as we will reload!
            folder?.Clear();
            if (!string.IsNullOrWhiteSpace(filterSearch))
            {
                return await GetSearchResults(folder, filterSearch, user);
            }

            if (folder?.IsShared ?? false)
            {
                return await GetSharedFileList(folder, user);
            }

            return await dataRetrievalService.GetFileList(folder, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFileList folder: {FullPathFromRoot} filter search: {FilterSearch} user: {DisplayName} FAILED",
                folder?.FullPathFromRoot, filterSearch, user?.DisplayName);
            throw;
        }
    }

    private async Task<Folder> GetSharedFileList(dynamic folder, User user)
    {
        var filter = $"sharedwith/any(c: c/userid eq '{user.Id}')";
        return await SearchIndex(folder, filter, user);
    }

    private async Task<Folder> GetSearchResults(dynamic folder, string searchText, User user)
    {
        var filter = $"search.ismatch('{searchText}*', 'filename', 'full', 'any') and ownedby eq '{user.Id}'";
        return await SearchIndex(folder, filter, user);
    }

    public async Task RestoreVersionOfBlob(string fileid, string versionId)
    {

        try
        {
            var containerClient = await dataRetrievalService.GetBlobContainerClient(null, MYDATACONTAINERNAME);
            // Get the name of the first blob in the container to use as the source.
            var sourceBlob = containerClient.GetBlobClient(fileid).WithVersion(versionId);

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
                global::Azure.Storage.Blobs.Models.BlobProperties sourceProperties = await sourceBlob.GetPropertiesAsync();
                //Console.WriteLine($"Lease state: {sourceProperties.LeaseState}");

                // Get a BlobClient representing the destination blob with a unique name.
                BlobClient destBlob =
                    containerClient.GetBlobClient(fileid);


                await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

                // Get the destination blob's properties and display the copy status.
                global::Azure.Storage.Blobs.Models.BlobProperties destProperties = await destBlob.GetPropertiesAsync();

                //Console.WriteLine($"Copy status: {destProperties.CopyStatus}");
                //Console.WriteLine($"Copy progress: {destProperties.CopyProgress}");
                //Console.WriteLine($"Completion time: {destProperties.CopyCompletedOn}");
                //Console.WriteLine($"Total bytes: {destProperties.ContentLength}");

                // Update the source blob's properties.
                sourceProperties = await sourceBlob.GetPropertiesAsync();

                if (sourceProperties.LeaseState == global::Azure.Storage.Blobs.Models.LeaseState.Leased)
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

    public async Task<long> GetUserUsedDataTotal(User user)
    {


        Folder rootFolder = new Folder()
        {
            Id = MyDataFolder.Id,
            Name = MyDataFolder.Name,
            IsShared = false
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

    public const string MYDATACONTAINERNAME = "datahub";



}