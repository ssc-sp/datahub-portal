using Datahub.Core.Data;
using Datahub.Core.Storage;

namespace Datahub.Infrastructure.Services.Storage
{
	public class DisabledCloudStorageManager : ICloudStorageManager
    {
        private const string DISABLED_TEXT = "---";
        private readonly CloudStorageProviderType _provider;
        private readonly string _displayName;

        public DisabledCloudStorageManager(CloudStorageProviderType provider, string displayName)
        {
            _provider = provider;
            _displayName = displayName;
        }

        public Task<List<string>> GetContainersAsync()
        {
            return Task.FromResult(new List<string>() { DISABLED_TEXT });
        }

        public bool AzCopyEnabled => throw new NotImplementedException();

        public bool DatabrickEnabled => throw new NotImplementedException();

        public CloudStorageProviderType ProviderType => _provider;

        public string DisplayName => _displayName;

        public Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string folderName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFileAsync(string container, string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFolderAsync(string container, string folderPath)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> DownloadFileAsync(string container, string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FileExistsAsync(string container, string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> GenerateSasTokenAsync(string container, int days)
        {
            throw new NotImplementedException();
        }

        public Task<DfsPage> GetDfsPagesAsync(string container, string folderPath, string? continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<StorageMetadata> GetStorageMetadataAsync(string container)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameFileAsync(string container, string oldFilePath, string newFilePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UploadFileAsync(string container, FileMetaData file, Action<long> progess)
        {
            throw new NotImplementedException();
        }

        public List<(string Placeholder, string Replacement)> GetSubstitutions(string projectAcronym, CloudStorageContainer container)
        {
            return new List<(string, string)>();
        }
    }
}
