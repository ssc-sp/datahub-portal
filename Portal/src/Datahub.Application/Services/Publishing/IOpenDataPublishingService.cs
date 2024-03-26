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
        Task<int> AddFilesToSubmission(OpenDataSubmission openDataSubmission, IEnumerable<FileMetaData> files, int? containerId, string containerName);
        Task<OpenDataPublishFile> UpdateFileUploadStatus(OpenDataPublishFile file, OpenDataPublishFileUploadStatus status, string? uploadMessage = null);
        Task<int> RefreshFileUploadStatuses(OpenDataSubmission? submission);
    }

    public class OpenDataPublishingException : Exception
    {
        public OpenDataPublishingException() : base() { }
        public OpenDataPublishingException(string? message) : base(message) { }
        public OpenDataPublishingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
