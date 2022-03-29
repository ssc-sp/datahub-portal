using Datahub.Metadata.DTO;
using Datahub.Portal.Data.Forms.ShareWorkflow;
using Entities = Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Portal.Services
{
    public interface IMetadataBrokerService
    {
        Task<Entities.MetadataProfile> GetProfile(string name);
        Task<FieldValueContainer> GetObjectMetadataValues(long objectMetadataId);
        Task<FieldValueContainer> GetObjectMetadataValues(string objectId);
        Task<Entities.ObjectMetadata> SaveMetadata(FieldValueContainer fieldValues, bool anonymous = false);
        Task<Entities.ObjectMetadata> GetMetadata(long objectMetadataId);
        Task<Entities.ObjectMetadata> GetMetadata(string objectId);
        Task<ApprovalForm> GetApprovalForm(int ApprovalFormId);
        Task DeleteApprovalForm(int approvalFormId);
        Task<int> SaveApprovalForm(ApprovalForm form);
        Task<List<string>> GetSuggestedEnglishKeywords(string text, int max);
        Task<List<string>> GetSuggestedFrenchKeywords(string text, int max);
        Task<List<SubjectKeyword>> GetSubjectKeywords(IEnumerable<string> subjectIds);
        Task UpdateCatalog(long objectId, Entities.MetadataObjectType dataType, string objectName, string location, int sector, int branch, string contact, string englishText, string frenchText);
        Task<List<CatalogObjectResult>> SearchCatalogEnglish(string searchText);
        Task<List<CatalogObjectResult>> SearchCatalogFrench(string searchText);
        Task<FieldDefinitions> GetFieldDefinitions();
    }    
}
