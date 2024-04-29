using Datahub.Application.Services.Metadata;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;

namespace Datahub.Infrastructure.Offline;

public class OfflineMetadataBrokerService : IMetadataBrokerService
{
    public Task<List<MetadataProfile>> GetProfiles()
    {
        throw new NotImplementedException();
    }

    public Task<MetadataProfile> GetProfile(string name)
    {
        throw new NotImplementedException();
    }

    public Task<FieldValueContainer> GetObjectMetadataValues(long objectMetadataId, string defaultMetadataId = null)
    {
        throw new NotImplementedException();
    }

    public Task<FieldValueContainer> GetObjectMetadataValues(string objectId, string defaultMetadataId = null)
    {
        throw new NotImplementedException();
    }

    public Task<ObjectMetadata> SaveMetadata(FieldValueContainer fieldValues, bool anonymous = false)
    {
        throw new NotImplementedException();
    }

    public Task<ObjectMetadata> GetMetadata(long objectMetadataId)
    {
        throw new NotImplementedException();
    }

    public Task<ObjectMetadata> GetMetadata(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateChildMetadata(string parentId, string childId, MetadataObjectType dataType, string location,
        bool includeCatalog)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalForm> GetApprovalForm(int approvalFormId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteApprovalForm(int approvalFormId)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveApprovalForm(ApprovalForm form)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetSuggestedEnglishKeywords(string text, int max)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetSuggestedFrenchKeywords(string text, int max)
    {
        throw new NotImplementedException();
    }

    public Task<List<SubjectKeyword>> GetSubjectKeywords(IEnumerable<string> subjectIds)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCatalog(long objectId, MetadataObjectType dataType, string englishName, string frenchName, string location,
        int sector, int branch, string contact, ClassificationType securityClass, string englishText, string frenchText,
        CatalogObjectLanguage language, int? projectId, bool anonymous = false)
    {
        throw new NotImplementedException();
    }

    public Task<List<CatalogObjectResult>> SearchCatalog(CatalogSearchRequest request, Func<CatalogObjectResult, bool> validateResult)
    {
        throw new NotImplementedException();
    }

    public Task<List<CatalogObjectResult>> GetCatalogGroup(Guid groupId)
    {
        throw new NotImplementedException();
    }

    public Task<FieldDefinitions> GetFieldDefinitions()
    {
        throw new NotImplementedException();
    }

    public Task<CatalogObjectResult> GetCatalogObjectByMetadataId(long metadataId)
    {
        throw new NotImplementedException();
    }

    public Task<CatalogObjectResult> GetCatalogObjectByObjectId(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsObjectCatalogued(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFromCatalog(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMultipleFromCatalog(IEnumerable<string> objectIds)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> GroupCatalogObjects(IEnumerable<string> objectIds)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetObjectCatalogGroup(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task<CatalogObjectLanguage?> GetCatalogObjectLanguage(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task<List<CatalogObjectResult>> GetProjectCatalogItems(int projectId)
    {
        throw new NotImplementedException();
    }

    public Task<ClassificationType?> GetObjectClassification(string objectId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateMetadata(Stream stream)
    {
        throw new NotImplementedException();
    }

    public Task<List<FieldChoice>> GetFieldChoices(string fieldName)
    {
        throw new NotImplementedException();
    }

    public Task<List<NameValuePair>> ListObjectFieldValues(string fieldName)
    {
        throw new NotImplementedException();
    }
}