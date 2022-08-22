using Datahub.Metadata.DTO;
using Entities = Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Metadata.Model;
using System;

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
            int sector, int branch, string contact, ClassificationType securityClass, string englishText, string frenchText, 
            CatalogObjectLanguage language);
        Task<List<CatalogObjectResult>> SearchCatalog(CatalogSearchRequest request, Func<CatalogObjectResult, bool> validateResult);
        Task<List<CatalogObjectResult>> GetCatalogGroup(Guid groupId);
        Task<FieldDefinitions> GetFieldDefinitions();
        Task<CatalogObjectResult> GetCatalogObjectByMetadataId(long metadataId);
        Task<CatalogObjectResult> GetCatalogObjectByObjectId(string objectId);
        Task<bool> IsObjectCatalogued(string objectId);
        Task DeleteFromCatalog(string objectId);
        Task DeleteMultipleFromCatalog(IEnumerable<string> objectIds);
        Task<Guid> GroupCatalogObjects(IEnumerable<string> objectIds);
        Task<List<string>> GetObjectCatalogGroup(string objectId);
    }
    
    public record CatalogSearchRequest
    (
        long LastPageId,
        int PageSize,
        bool IsFrench,
        List<string> Keywords,
        List<CatalogObjectLanguage> Languages,
        List<ClassificationType> Classifications,
        List<MetadataObjectType> ObjectTypes,
        List<int> Sectors,
        List<int> Branches 
    );
}
