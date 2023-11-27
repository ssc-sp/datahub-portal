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
    }
}
