using Datahub.Core.Data;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Security;
using Datahub.Portal.Pages.Workspace.Storage.ResourcePages;
using Google;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using GObject = Google.Apis.Storage.v1.Data.Object;

namespace Datahub.Infrastructure.Services.Storage
{
	public class GoogleCloudStorageManager : ICloudStorageManager
    {
        private const int PAGE_SIZE = 100;
        private const string EMPTY_FOLDER_CONTENT_TYPE = "text/plain";
        private const long MAX_FILE_SIZE = 5L * 1024 * 1024 * 1024 * 1024; // 5 TiB

        public bool AzCopyEnabled => false;

        public bool DatabrickEnabled => false;

        public CloudStorageProviderType ProviderType => CloudStorageProviderType.GCP;

        public string DisplayName => _displayName;

        private readonly ILogger<GoogleCloudStorageManager> _logger;

        private readonly string _projectId;
        private readonly string _jsonCredentials;
        private readonly string _displayName;

        public GoogleCloudStorageManager(ILoggerFactory loggerFactory, string projectId, string jsonCredentials, string displayName)
        {
            _logger = loggerFactory.CreateLogger<GoogleCloudStorageManager>();
            _projectId = projectId;
            _jsonCredentials = jsonCredentials;
            _displayName = displayName;
        }

        private GoogleCredential GetCredential()
        {
            var creds = GoogleCredential.FromJson(_jsonCredentials);
            return creds;
        }

        private async Task<StorageClient> CreateStorageClientAsync()
        {
            var creds = GetCredential();
            return await StorageClient.CreateAsync(creds);
        }

        private UrlSigner CreateUrlSigner()
        {
            var creds = GetCredential();
            return UrlSigner.FromCredential(creds);
        }

        public async Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string folderName)
        {
            using var client = await CreateStorageClientAsync();
            var targetObjectName = $"{NormalizeFolderPath(currentWorkingDirectory)}{folderName}/";
            var options = new UploadObjectOptions();

            try
            {
                using var stream = new MemoryStream();
                var result = await client.UploadObjectAsync(container, targetObjectName, EMPTY_FOLDER_CONTENT_TYPE, stream, options);
                return await Task.FromResult(result != null);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        private async Task<bool> DeleteObjectAsync(string container, string filePath)
        {
            using var client = await CreateStorageClientAsync();
            var options = new DeleteObjectOptions();

            try
            {
                await client.DeleteObjectAsync(container, filePath, options);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeleteFileAsync(string container, string filePath)
        {
            return await DeleteObjectAsync(container, filePath);
        }

        public async Task<bool> DeleteFolderAsync(string container, string folderPath)
        {
            return await DeleteObjectAsync(container, NormalizeFolderPath(folderPath));
        }

        public async Task<Uri> DownloadFileAsync(string container, string filePath)
        {
            var signer = CreateUrlSigner();
            var urlValidTime = TimeSpan.FromDays(1);
            var uriString = await signer.SignAsync(container, filePath, urlValidTime);
            return await Task.FromResult(new Uri(uriString));
        }

        public async Task<bool> FileExistsAsync(string container, string filePath)
        {
            using var client = await CreateStorageClientAsync();
            var options = new GetObjectOptions();

            try
            {
                var obj = await client.GetObjectAsync(container, filePath, options);
                return await Task.FromResult(obj != null);
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return await Task.FromResult(false);
                }
                else
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
            }
        }

        public Task<Uri> GenerateSasTokenAsync(string container, int days)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetContainersAsync()
        {
            using var storageClient = await CreateStorageClientAsync();
            var options = new ListBucketsOptions() { PageSize = PAGE_SIZE };
            var bucketNames = new List<string>();
            Page<Bucket>? buckets = null;

            while (buckets == null || buckets.NextPageToken != null)
            {
                options.PageToken = buckets?.NextPageToken;
                buckets = await storageClient.ListBucketsAsync(_projectId, options).ReadPageAsync(PAGE_SIZE);
                bucketNames.AddRange(buckets.Select(b => b.Id));
            }

            return await Task.FromResult(bucketNames);
        }

        private static bool IsFolder(string objName) => objName.EndsWith("/");
        private static bool IsFolder(GObject obj) => IsFolder(obj.Name);

        private static string GetObjectName(GObject obj) => obj.Name.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[^1];

        private static string NormalizeFolderPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || folderPath == "/")
            {
                return string.Empty;
            }
            else if (folderPath.EndsWith('/'))
            {
                return folderPath;
            }
            else
            {
                return $"{folderPath}/";
            }
        }

        public async Task<DfsPage> GetDfsPagesAsync(string container, string folderPath, string? continuationToken = null)
        {
            using var storageClient = await CreateStorageClientAsync();
            var options = new ListObjectsOptions() { PageSize = PAGE_SIZE };
            var folders = new List<string>();
            var files = new List<FileMetaData>();

            folderPath = NormalizeFolderPath(folderPath);

            try
            {
                Page<GObject>? objects = null;

                // continuationToken from the interface doesn't actually do anything... for now just handle API pages internally
                while (objects == null || objects.NextPageToken != null)
                {
                    objects = await storageClient.ListObjectsAsync(container, folderPath, options).ReadPageAsync(PAGE_SIZE);

                    foreach (var obj in objects)
                    {
                        if (obj.Name.Equals(folderPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            // exclude the folder from being listed inside itself
                            continue;
                        }
                        else if (obj.Name[folderPath.Length..^1].Contains('/'))
                        {
                            // exclude items in subfolders
                            continue;
                        }

                        if (IsFolder(obj))
                        {
                            folders.Add(folderPath + GetObjectName(obj));
                        }
                        else
                        {
                            files.Add(new()
                            {
                                id = obj.ETag,
                                name = GetObjectName(obj),
                                folderpath = folderPath,
                                filesize = obj.Size?.ToString(),
                                lastmodifiedts = obj.Updated ?? default,
                            });
                        }
                    }
                }
            }
            catch (GoogleApiException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }

            return new(folders, files, continuationToken);
        }

        public Task<StorageMetadata> GetStorageMetadataAsync(string container)
        {
            var metadata = new StorageMetadata()
            {
                Container = container,
                //TODO additional fields
            };

            return Task.FromResult(metadata);
        }

        public async Task<bool> RenameFileAsync(string container, string oldFilePath, string newFilePath)
        {
            using var client = await CreateStorageClientAsync();
            GObject? dest = null;

            try
            {
                // GCP API doesn't have a rename operation - objects must be copied and deleted
                dest = await client.CopyObjectAsync(container, oldFilePath, container, newFilePath);
                // note: since dest is a newly created copy, it will have creation/last update timestamp when it was renamed
                // changing these is not possible; the operation appears to succeed, but only updates the last update timestamp (i.e. doesn't back-date it)
                await client.DeleteObjectAsync(container, oldFilePath);
                return await Task.FromResult(true);
            }
            catch
            {
                if (dest != null)
                {
                    // if dest was created but it failed to delete the old file, likely deleting dest will also fail
                    await client.DeleteObjectAsync(dest);
                }
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> UploadFileAsync(string container, FileMetaData file, Action<long> progess)
        {
            using var client = await CreateStorageClientAsync();

            var destinationFilePath = $"{NormalizeFolderPath(file.folderpath)}{file.filename}";
            var contentType = file.BrowserFile.ContentType;
            var options = new UploadObjectOptions();

            try
            {
                using var stream = file.BrowserFile.OpenReadStream(MAX_FILE_SIZE);
                var progressHandler = new GoogleUploadProgressHandler(progess);
                var createdObj = await client.UploadObjectAsync(container, destinationFilePath, contentType, stream, options, default, progressHandler);
                return await Task.FromResult(createdObj != null);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public List<(string Placeholder, string Replacement)> GetSubstitutions(string projectAcronym, CloudStorageContainer container)
        {
            return new List<(string, string)>
        {
            (ResourceSubstitutions.ProjectAcronym, projectAcronym),
            (ResourceSubstitutions.GCPProjectId, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.GCP_ProjectId)),
            (ResourceSubstitutions.GCPAccountKey, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.GCP_Json)),
            (ResourceSubstitutions.ContainerName, container.Name)
        };
        }
    }
}
