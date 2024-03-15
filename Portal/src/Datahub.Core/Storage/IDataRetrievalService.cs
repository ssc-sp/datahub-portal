using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Datahub.Core.Data;
using Microsoft.Graph.Models;

namespace Datahub.Application.Services.Storage;

public interface IDataRetrievalService
{
    string GetBlobContainerName();
    Task<string> GetStorageConnString();
    Task<string> GetProjectConnectionString(string accountName);
    Task<Datahub.Core.Data.Folder> GetProjectFileList(string container, string project, User user);
    Task<BlobContainerClient> GetBlobContainerClient(string project, string containerName);
    Task<Uri> GetUserDelegationSasBlob(string container, string fileName, string projectUploadCode, int daysValidity = 1);
    Task<Uri> GetDownloadAccessToSasBlob(string container, string fileName, string projectUploadCode, int daysValidity = 1);
    Task<Uri> GenerateSasToken(string container, string projectUploadCode, int daysValidity, bool containerLevel = false);
    Task<Uri> DownloadFile(string container, FileMetaData file, string projectUploadCode);
    Task<List<Datahub.Core.Data.Version>> GetFileVersions(string fileId);
    Task<List<string>> GetSubFolders(DataLakeFileSystemClient fileSystemClient, string folderName);
    Task<List<string>> GetAllFolders(string rootFolderName, User user);
    Task<Datahub.Core.Data.Folder> GetFileList(Datahub.Core.Data.Folder folder, User user, bool onlyFolders = false, bool recursive = false);
    Task<StorageMetadata> GetStorageMetadata(string project);
    Task<List<string>> ListContainers(string projectAcronym, User user);
    Task<List<FileMetaData>> GetStorageBlobFiles(string projectAcronym, string container, User user);
    Task<(List<string> A, List<FileMetaData> B, string C)> GetStorageBlobPagesAsync(string projectAcronym, string containerName, User user, string prefix, string continuationToken = default);
    Task<List<string>> GetProjectContainersAsync(string projectAcronymParam, User user);
    Task<bool> StorageBlobExistsAsync(string filename, string projectAcronym, string containerName);
}