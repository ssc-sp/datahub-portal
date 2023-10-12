using Datahub.Core.Data;

namespace Datahub.Application.Services.Storage;

public interface ICloudStorageManager
{
    Task<List<string>> GetContainersAsync();

    Task<Uri> GenerateSasTokenAsync(string container, int days);

    Task<DfsPage> GetDfsPagesAsync(string container, string folderPath, string? continuationToken = default);

    Task<bool> FileExistsAsync(string container, string filePath);

    Task<Uri> DownloadFileAsync(string container, string filePath);
    Task<bool> UploadFileAsync(string container, FileMetaData file, Action<long> progess);

    Task<bool> DeleteFileAsync(string container, string filePath);
    Task<bool> RenameFileAsync(string container, string oldFilePath, string newFilePath);

    Task<bool> DeleteFolderAsync(string container, string folderPath);

    Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string folderName);

    Task<StorageMetadata> GetStorageMetadataAsync(string container);

    bool AzCopyEnabled { get; }
    bool DatabrickEnabled { get; }
}

public record DfsPage(List<string> Folders, List<FileMetaData> Files, string ContinuationToken);
