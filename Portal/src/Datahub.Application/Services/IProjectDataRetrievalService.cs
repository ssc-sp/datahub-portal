using Datahub.Core.Data;

namespace Datahub.Application.Services;

public interface IProjectDataRetrievalService
{
    public Task<(List<string>, List<FileMetaData>, string)> GetStorageBlobPagesAsync(string projectAcronym,
        string containerName, Microsoft.Graph.User user, string prefix, string? continuationToken = default);

    public Task<bool> StorageBlobExistsAsync(string filename, string projectAcronym, string containerName);
    public Task<Uri> DownloadFile(string container, FileMetaData file, string projectAcronym);
    public Task<List<string>> GetProjectContainersAsync(string projectAcronym);
}