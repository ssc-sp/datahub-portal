using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;

namespace Datahub.Application.Services.Publishing
{
	public interface IOpenDataPublishingService
    {
        Task<List<OpenDataSubmission>> GetOpenDataSubmissionsAsync(int workspaceId);
        Task<OpenDataSubmission> GetOpenDataSubmissionAsync(long submissionId);
        Task<List<OpenDataSubmission>> GetAvailableOpenDataSubmissionsForWorkspaceAsync(int workspaceId);
        Task<TbsOpenGovSubmission> UpdateTbsOpenGovSubmission(TbsOpenGovSubmission submission);
        Task<OpenDataSubmission> CreateOpenDataSubmission(OpenDataSubmissionBasicInfo openDataSubmissionBasicInfo);
        Task AddFilesToSubmission(OpenDataSubmission openDataSubmission, IEnumerable<FileMetaData> files, int? containerId, string containerName);
        Task<OpenDataPublishFile> UpdateFileUploadStatus(OpenDataPublishFile file, OpenDataPublishFileUploadStatus status, string? uploadMessage = null);
        event Func<OpenDataPublishFile, Task> FileUploadStatusUpdated;
    }
}
