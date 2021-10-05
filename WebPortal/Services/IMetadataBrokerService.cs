using NRCan.Datahub.Metadata.DTO;
using NRCan.Datahub.Portal.Data.Forms.ShareWorkflow;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services
{
    public interface IMetadataBrokerService
    {
        Task<ObjectMetadataContext> GetMetadataContext(string objectId);
        Task SaveMetadata(string objectId, int metadataVersionId, FieldValueContainer fieldValues);
        Task<ApprovalForm> GetApprovalForm(int ApprovalFormId);
        Task<int> SaveApprovalForm(ApprovalForm form);
        Task<List<string>> GetSuggestedEnglishKeywords(string text, int max);
        Task<List<string>> GetSuggestedFrenchKeywords(string text, int max);
        Task DeleteApprovalForm(int approvalFormId);
    }
}
