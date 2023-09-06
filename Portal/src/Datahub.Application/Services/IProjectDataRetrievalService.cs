using Datahub.Core.Data;

namespace Datahub.Application.Services;

public interface IProjectDataRetrievalService
{
    Task<string> GetProjectStorageAccountKey(string projectAcronym);
    string GetProjectStorageAccountName(string projectAcronym);

    Task<List<string>> GetContainersAsync(string projectAcronym);
    Task<Uri> GenerateSasTokenAsync(string projectAcronym, string containerName, int days);

    Task<(List<string> Folders, List<FileMetaData> Files, string Token)> GetDfsPagesAsync(string projectAcronym, string containerName, 
        string folderPath, string? continuationToken = default);

    Task<bool> FileExistsAsync(string projectAcronym, string containerName, string filePath);

    Task<Uri> DownloadFileAsync(string projectAcronym, string containerName, string filePath);
    Task<bool> UploadFileAsync(string projectAcronym, string containerName, FileMetaData file, Action<long> progess);

    Task<bool> DeleteFileAsync(string projectAcronym, string containerName, string filePath);
    Task<bool> RenameFileAsync(string projectAcronym, string containerName, string oldFilePath, string newFilePath);

    Task<bool> DeleteFolderAsync(string projectAcronym, string containerName, string folderPath);

    Task<bool> CreateFolderAsync(string projectAcronym, string containerName, string currentWorkingDirectory, string folderName);

    Task<StorageMetadata> GetStorageMetadataAsync(string projectAcronym, string containerName);
}
