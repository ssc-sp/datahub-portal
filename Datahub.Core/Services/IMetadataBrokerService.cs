using Datahub.Metadata.DTO;
using Entities = Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IMetadataBrokerService
    {
        Task<Entities.MetadataProfile> GetProfile(string name);
        Task<FieldValueContainer> GetObjectMetadataValues(long objectMetadataId, string defaultMetadataId = null);
        Task<FieldValueContainer> GetObjectMetadataValues(string objectId, string defaultMetadataId = null);
        Task<Entities.ObjectMetadata> SaveMetadata(FieldValueContainer fieldValues, bool anonymous = false);
        Task<Entities.ObjectMetadata> GetMetadata(long objectMetadataId);
        Task<Entities.ObjectMetadata> GetMetadata(string objectId);
        Task<Entities.ApprovalForm> GetApprovalForm(int ApprovalFormId);
        Task DeleteApprovalForm(int approvalFormId);
        Task<int> SaveApprovalForm(Entities.ApprovalForm form);
        Task<List<string>> GetSuggestedEnglishKeywords(string text, int max);
        Task<List<string>> GetSuggestedFrenchKeywords(string text, int max);
        Task<List<SubjectKeyword>> GetSubjectKeywords(IEnumerable<string> subjectIds);
        Task UpdateCatalog(long objectId, Entities.MetadataObjectType dataType, string objectName, string location, 
            int sector, int branch, string contact, string securityClass, string englishText, string frenchText);
        Task<List<CatalogObjectResult>> SearchCatalogEnglish(string searchText);
        Task<List<CatalogObjectResult>> SearchCatalogFrench(string searchText);
        Task<FieldDefinitions> GetFieldDefinitions();
        Task<CatalogObjectResult> GetCatalogObjectByMetadataId(long metadataId);
        Task<CatalogObjectResult> GetCatalogObjectByObjectId(string objectId);
    }    
}
