using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Application.Services.Publishing
{
    public interface IOpenDataPublishingService
    {
        Task<List<OpenDataSubmission>> GetOpenDataSubmissionsAsync(int workspaceId);
        Task<OpenDataSubmission> GetOpenDataSubmissionAsync(long submissionId);
        Task<List<OpenDataSubmission>> GetAvailableOpenDataSubmissionsForWorkspaceAsync(int workspaceId);
        Task<TbsOpenGovSubmission> UpdateTbsOpenGovSubmission(TbsOpenGovSubmission submission);
        Task<OpenDataSubmission> CreateOpenDataSubmission(OpenDataSubmissionBasicInfo openDataSubmissionBasicInfo);
        Task<int> AddFilesToSubmission(OpenDataSubmission openDataSubmission, IEnumerable<FileMetaData> files, int? containerId);
    }

    public class OpenDataPublishingException : Exception
    {
        public OpenDataPublishingException() : base() { }
        public OpenDataPublishingException(string? message) : base(message) { }
        public OpenDataPublishingException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
