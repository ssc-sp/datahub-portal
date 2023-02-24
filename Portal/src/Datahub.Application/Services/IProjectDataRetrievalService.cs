using Datahub.Core.Data;

namespace Datahub.Application.Services;

public interface IProjectDataRetrievalService
{
    public Task<(List<string>, List<FileMetaData>, string)> GetStorageBlobPagesAsync(string projectAcronym,
        string containerName, Microsoft.Graph.User user, string currentWorkingDirectory, string? continuationToken = default);
    
    public Task<(List<string>, List<FileMetaData>, string)> GetDfsPagesAsync(string projectAcronym,
        string containerName, Microsoft.Graph.User user, string currentWorkingDirectory, string? continuationToken = default);

    public Task<bool> StorageBlobExistsAsync(string filename, string projectAcronym, string containerName);
    public Task<Uri> DownloadFile(string container, FileMetaData file, string projectAcronym);
    public Task<List<string>> GetProjectBlobContainersAsync(string projectAcronym);
    public Task<List<string>> GetProjectDataLakeContainersAsync(string projectAcronym, Microsoft.Graph.User user);
}
