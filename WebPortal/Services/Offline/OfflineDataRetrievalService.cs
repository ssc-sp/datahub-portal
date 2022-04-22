using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graph;
using Datahub.Core.Data;
using Datahub.Core.Services;
using System;
using Microsoft.AspNetCore.Components;
using Datahub.Portal.Services.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;

namespace Datahub.Portal.Services.Offline
{
    public class OfflineDataRetrievalService : IDataRetrievalService
    {

        private static Uri FakeUri => new("http://example.com");

        public async Task<Uri> DownloadFile(string container, FileMetaData file, string projectUploadCode)
        {
            return await Task.FromResult(FakeUri);
        }

        public async Task<Uri> GenerateSasToken(string container, string projectUploadCode, int daysValidity, bool containerLevel = false)
        {
            return await Task.FromResult(FakeUri);
        }

        public async Task<List<string>> GetAllFolders(string rootFolderName, User user)
        {
            return await Task.FromResult(new List<string>());
        }

        public async Task<BlobContainerClient> GetBlobContainerClient(string project, string containerName)
        {
            return await Task.FromResult(default(BlobContainerClient));
        }

        public string getBlobContainerName()
        {
            return string.Empty;
        }

        public async Task<Uri> GetDownloadAccessToSasBlob(string container, string fileName, string projectUploadCode, int daysValidity = 1)
        {
            return await Task.FromResult(FakeUri);
        }

        public async Task<Core.Data.Folder> GetFileList(Core.Data.Folder folder, User user, bool onlyFolders = false, bool recursive = false)
        {
            return await Task.FromResult(new Core.Data.Folder());
        }

        public async Task<List<Core.Data.Version>> GetFileVersions(string fileId)
        {
            return await Task.FromResult(new List<Core.Data.Version>());
        }

        public async Task<string> GetProjectConnectionString(string accountName)
        {
            return await Task.FromResult(string.Empty);
        }

        public async Task<List<string>> GetProjectContainersAsync(string projectAcronymParam, User user)
        {
            return await Task.FromResult(new List<string>());
        }

        public async Task<Core.Data.Folder> GetProjectFileList(string container, string project, User user)
        {
            return await Task.FromResult(default(Core.Data.Folder));
        }

        public async Task<List<FileMetaData>> GetStorageBlobFiles(string projectAcronym, string container, User user)
        {
            return await Task.FromResult(new List<FileMetaData>());
        }

        public async Task<(List<string>, List<FileMetaData>, string)> GetStorageBlobPagesAsync(string projectAcronym, string containerName, User user, string prefix, string continuationToken = null)
        {
            return await Task.FromResult((new List<string>(), new List<FileMetaData>(), string.Empty));
        }

        public async Task<string> getStorageConnString()
        {
            return await Task.FromResult(string.Empty);
        }

        public async Task<StorageMetadata> GetStorageMetadata(string project)
        {
            return await Task.FromResult(new StorageMetadata());
        }

        public async Task<List<string>> GetSubFolders(DataLakeFileSystemClient fileSystemClient, string folderName)
        {
            return await Task.FromResult(new List<string>());
        }

        public async Task<Uri> GetUserDelegationSasBlob(string container, string fileName, string projectUploadCode, int daysValidity = 1)
        {
            return await Task.FromResult(FakeUri);
        }

        public async Task<List<string>> ListContainers(string projectAcronym, User user)
        {
            return await Task.FromResult(new List<string>());
        }

        public async Task<bool> StorageBlobExistsAsync(string filename, string projectAcronym, string containerName)
        {
            return await Task.FromResult(true);
        }
    }
}
