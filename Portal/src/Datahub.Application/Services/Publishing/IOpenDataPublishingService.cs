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

        // Publishing Blocklist methods
        /// <summary>
        /// Gets only active blocklist entries
        /// </summary>
        Task<List<OpenGovPublishingBlocklist>> GetActiveBlocklistEntriesAsync();

        /// <summary>
        /// Gets a specific blocklist entry by ID
        /// </summary>
        Task<OpenGovPublishingBlocklist> GetBlocklistEntryAsync(int id);

        /// <summary>
        /// Checks if a user is blocked based on Email Domain or department
        /// </summary>
        Task<bool> IsUserBlockedAsync(string emailDomain, string? departmentName = null);

        /// <summary>
        /// Checks if publishing is blocked for a workspace by checking the workspace lead's email domain
        /// Results are cached to avoid repeated database calls
        /// </summary>
        Task<bool> IsPublishingBlockedForWorkspaceAsync(string workspaceAcronym);

        /// <summary>
        /// Adds a new blocklist entry
        /// </summary>
        Task<OpenGovPublishingBlocklist> AddBlocklistEntryAsync(string departmentName, string emailHostname, string notes);

        /// <summary>
        /// Updates an existing blocklist entry
        /// </summary>
        Task<OpenGovPublishingBlocklist> UpdateBlocklistEntryAsync(int id, string departmentName, string emailHostname, string notes);

        /// <summary>
        /// Soft deletes a blocklist entry (marks as deleted)
        /// </summary>
        Task DeleteBlocklistEntryAsync(int id);
    }
}
