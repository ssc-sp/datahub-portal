using System.Text.Json;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.CatalogSearch;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Entities = Datahub.Metadata.Model;

namespace Datahub.Infrastructure.Services.Metadata;

public class MetadataBrokerService : IMetadataBrokerService
{
    private const int MaxKeywordResults = 50;
    private const string DefaultSource = "default";

    private readonly IDbContextFactory<MetadataDbContext> contextFactory;
    private readonly ILogger<MetadataBrokerService> logger;
    private readonly IDatahubAuditingService auditingService;
    private readonly ICatalogSearchEngine catalogSearchEngine;

    public MetadataBrokerService(IDbContextFactory<MetadataDbContext> contextFactory, ILogger<MetadataBrokerService> logger,
        IDatahubAuditingService auditingService, ICatalogSearchEngine catalogSearchEngine)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
        this.auditingService = auditingService;
        this.catalogSearchEngine = catalogSearchEngine;
    }

    public async Task<List<Entities.MetadataProfile>> GetProfiles()
    {
        using var ctx = contextFactory.CreateDbContext();
        return await GetProfiles(ctx);
    }

    public async Task<MetadataProfile> GetProfile(string name)
    {
        using var ctx = contextFactory.CreateDbContext();
        return await ctx.Profiles
                        .Include(p => p.Sections)
                        .ThenInclude(s => s.Fields)
                        .AsSingleQuery()
                        .FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<FieldValueContainer> GetObjectMetadataValues(long objectMetadataId, string defaultMetadataId)
    {
        using var ctx = contextFactory.CreateDbContext();

        // retrieve the object metadata
        var objectMetadata = await GetObjectMetadata(ctx, objectMetadataId);

        // retrieve the field definitions
        var metadataDefinitions = await (objectMetadata == null ? GetLatestMetadataDefinition(ctx) : GetMetadataDefinition(ctx, objectMetadata.MetadataVersionId));

        // retrieve and clone the field values
        var fieldValues = CloneFieldValues(objectMetadata?.FieldValues ?? await CloneMetadataValues(ctx, defaultMetadataId));

        return new FieldValueContainer(objectMetadataId, objectMetadata?.ObjectId_TXT, metadataDefinitions, fieldValues);
    }

    public async Task<FieldValueContainer> GetObjectMetadataValues(string objectId, string defaultMetadataId)
    {
        using var ctx = contextFactory.CreateDbContext();

        // retrieve the object metadata
        var objectMetadata = await GetObjectMetadata(ctx, objectId);

        // retrieve the field definitions
        var metadataDefinitions = await (objectMetadata == null ? GetLatestMetadataDefinition(ctx) : GetMetadataDefinition(ctx, objectMetadata.MetadataVersionId));

        // retrieve and clone the field values
        var fieldValues = CloneFieldValues(objectMetadata?.FieldValues ?? await CloneMetadataValues(ctx, defaultMetadataId));

        return new FieldValueContainer(objectMetadata?.ObjectMetadataId ?? 0, objectId, metadataDefinitions, fieldValues);
    }

    public async Task<ObjectMetadata> SaveMetadata(FieldValueContainer fieldValues, bool anonymous = false)
    {
        if (fieldValues.ObjectId == null)
            throw new ArgumentException("Expected 'ObjectId' in parameter fieldValues.");

        if (fieldValues.Definitions == null)
            throw new ArgumentException("Expected 'Definitions' in parameter fieldValues.");

        var objectId = fieldValues.ObjectId;
        var metadataVersionId = fieldValues.Definitions.MetadataVersion;

        using var ctx = contextFactory.CreateDbContext();

        var transation = ctx.Database.BeginTransaction();
        try
        {
            // fetch the existing metadata object or create a new one
            var current = await FetchObjectMetadata(ctx, objectId) ?? await CreateNewObjectMetadata(ctx, objectId, metadataVersionId);

            // hash the new values by FieldDefinitionId
            var newValues = fieldValues.ToDictionary(v => v.FieldDefinitionId);

            // hash the existing values by FieldDefinitionId
            var currentValues = current.FieldValues.ToDictionary(v => v.FieldDefinitionId);

            foreach (var editedValue in fieldValues)
            {
                // add, update or delete values
                if (currentValues.TryGetValue(editedValue.FieldDefinitionId, out ObjectFieldValue currentValue))
                {
                    if (currentValue.Value_TXT != editedValue.Value_TXT)
                    {
                        if (string.IsNullOrEmpty(editedValue.Value_TXT))
                        {
                            // delete if cleared
                            ctx.ObjectFieldValues.Remove(currentValue);
                        }
                        else
                        {
                            // update value
                            currentValue.Value_TXT = editedValue.Value_TXT;
                            ctx.ObjectFieldValues.Update(currentValue);
                        }
                    }
                }
                else
                {
                    // add new value if not null or empty
                    if (!string.IsNullOrEmpty(editedValue.Value_TXT))
                    {
                        editedValue.ObjectMetadataId = current.ObjectMetadataId;
                        ctx.ObjectFieldValues.Add(editedValue);
                    }
                }
            }

            // delete removed values
            foreach (var currentValue in current.FieldValues)
            {
                if (!newValues.ContainsKey(currentValue.FieldDefinitionId))
                {
                    ctx.ObjectFieldValues.Remove(currentValue);
                }
            }

            // save the changes
            await ctx.TrackSaveChangesAsync(auditingService, anonymous);

            transation.Commit();

            return current;
        }
        catch (Exception)
        {
            transation.Rollback();
            throw;
        }
    }

    public async Task<bool> CreateChildMetadata(string parentId, string childId, Entities.MetadataObjectType dataType, string location, bool includeCatalog)
    {
        try
        {
            // read child catalog
            using var ctx = await contextFactory.CreateDbContextAsync();
            if (await CatalogExists(ctx, childId))
                return false;

            // read parent catalog
            var parentCatalog = await GetCatalogObjectCopy(ctx, parentId);
            if (parentCatalog is null)
                return false;

            // save the child metadata
            var childFields = await GetObjectMetadataValues(childId, parentId);
            var childMetadata = await SaveMetadata(childFields);

            // save the new catalog entry
            var childCatalog = parentCatalog.Clone();
            childCatalog.CatalogObjectId = 0;
            childCatalog.ObjectMetadata = null;
            childCatalog.ObjectMetadataId = childMetadata.ObjectMetadataId;
            childCatalog.DataType = dataType;
            childCatalog.Location_TXT = location;

            ctx.CatalogObjects.Add(childCatalog);
            await ctx.TrackSaveChangesAsync(auditingService);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Create Child Metadata failed!");
            return false;
        }
    }

    public async Task<Entities.ApprovalForm> GetApprovalForm(int approvalFormId)
    {
        using var ctx = contextFactory.CreateDbContext();
        return await GetApprovalFormEntity(ctx, approvalFormId);
    }

    public async Task<int> SaveApprovalForm(ApprovalForm form)
    {
        using var ctx = contextFactory.CreateDbContext();

        // this is calculated field to simplify the doc generation
        UpdateRequiresBlanketApproval(form);

        if (form.ApprovalFormId == 0)
        {
            ctx.ApprovalForms.Add(form);
        }
        else
        {
            ctx.Attach(form);
            ctx.ApprovalForms.Update(form);
        }

        await ctx.TrackSaveChangesAsync(auditingService);

        return form.ApprovalFormId;
    }

    public async Task<List<string>> GetSuggestedEnglishKeywords(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }
        else
        {
            using var ctx = contextFactory.CreateDbContext();
            return await ctx.Keywords
                .Where(e => e.English_TXT.StartsWith(text))
                .OrderByDescending(e => e.Frequency)
                .Select(e => e.English_TXT)
                .Take(max)
                .ToListAsync();
        }
    }

    public async Task<List<string>> GetSuggestedFrenchKeywords(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }
        else
        {
            using var ctx = contextFactory.CreateDbContext();
            return await ctx.Keywords
                .Where(e => e.French_TXT.StartsWith(text))
                .OrderByDescending(e => e.Frequency)
                .Select(e => e.French_TXT)
                .Take(max)
                .ToListAsync();
        }
    }

    public async Task<List<SubjectKeyword>> GetSubjectKeywords(IEnumerable<string> subjectIds)
    {
        var keywords = new List<SubjectKeyword>();
        foreach (var subjectId in subjectIds)
        {
            var subject = await GetSubject(subjectId);
            if (subject != null)
            {
                keywords.AddRange(subject.SubSubjects.Select(s => new SubjectKeyword(s.Name_English_TXT, s.Name_French_TXT)));
            }
        }
        return keywords;
    }

    public async Task UpdateCatalog(long objectMetadataId, MetadataObjectType dataType, string englishName, string frenchName,
        string location, int sector, int branch, string contact, ClassificationType securityClass, string englishText, string frenchText,
        CatalogObjectLanguage language, int? projectId, bool anonymous = false)
    {
        using var ctx = contextFactory.CreateDbContext();

        var transation = ctx.Database.BeginTransaction();
        try
        {
            // get catalog objects
            var catalogObjects = await ctx.CatalogObjects.Where(e => e.ObjectMetadataId == objectMetadataId).ToListAsync();

            CatalogObject catalogObject = catalogObjects.FirstOrDefault() ?? new();
            {
                catalogObject.ObjectMetadataId = objectMetadataId;
                catalogObject.DataType = dataType;
                catalogObject.Name_TXT = englishName;
                catalogObject.Name_French_TXT = frenchName;
                catalogObject.Location_TXT = location;
                catalogObject.Sector_NUM = sector;
                catalogObject.Branch_NUM = branch;
                catalogObject.Contact_TXT = contact;
                catalogObject.Classification_Type = securityClass;
                catalogObject.Search_English_TXT = englishText;
                catalogObject.Search_French_TXT = frenchText;
                catalogObject.Language = language;
                catalogObject.ProjectId = projectId;
            }

            if (catalogObject.CatalogObjectId == 0)
                ctx.CatalogObjects.Add(catalogObject);
            else
                ctx.CatalogObjects.Update(catalogObject);

            await ctx.TrackSaveChangesAsync(auditingService, anonymous);

            transation.Commit();

            try
            {
                // update search indexes
                var id = $"{catalogObject.CatalogObjectId}";
                UpdateCatalogIndex(id, englishName, englishText, false);
                UpdateCatalogIndex(id, frenchName, frenchText, true);
            }
            catch (Exception ex)
            {
                logger.LogError("Indexing catalog object failed!", ex);
            }
        }
        catch (Exception)
        {
            transation.Rollback();
            throw;
        }
    }

    public async Task<FieldDefinitions> GetFieldDefinitions()
    {
        using var ctx = contextFactory.CreateDbContext();
        return await GetLatestMetadataDefinition(ctx);
    }

    public async Task<List<CatalogObjectResult>> SearchCatalog(CatalogSearchRequest request, Func<CatalogObjectResult, bool> validateResult)
    {
        logger.LogInformation(">>> SearchCatalog start...");

        using var ctx = contextFactory.CreateDbContext();

        var query = ctx.CatalogObjects
            .Include(e => e.ObjectMetadata)
            .ThenInclude(s => s.FieldValues)
            .AsQueryable();

        var containsKeywords = request.Keywords.Count > 0;
        var pageSize = request.PageSize;

        List<long> hits = new();
        if (containsKeywords)
        {
            var kwSearch = request.IsFrench ? catalogSearchEngine.GetMetadataFrenchSearchEngine() : catalogSearchEngine.GetMetadataEnglishSearchEngine();

            hits = kwSearch.SearchDocuments(string.Join(" ", request.Keywords.Select(s => s.ToLower())), MaxKeywordResults)
                           .Select(long.Parse)
                           .ToList();

            pageSize = hits.Count;

            query = query.Where(e => hits.Contains(e.CatalogObjectId));
        }

        if (request.Classifications.Count > 0)
            query = query.Where(e => request.Classifications.Contains(e.Classification_Type));

        if (request.Languages.Count > 0)
            query = query.Where(e => request.Languages.Contains(e.Language));

        if (request.ObjectTypes.Count > 0)
            query = query.Where(e => request.ObjectTypes.Contains(e.DataType));

        if (request.Sectors.Count > 0)
            query = query.Where(e => request.Sectors.Contains(e.Sector_NUM));

        if (request.Branches.Count > 0)
            query = query.Where(e => request.Branches.Contains(e.Branch_NUM));

        if (!containsKeywords)
            query = query.Where(e => e.CatalogObjectId > request.LastPageId);

        var definitions = await GetLatestMetadataDefinition(ctx);

        var results = query.Select(e => TransformCatalogObject(e, definitions))
                      .Where(validateResult)
                      .Take(pageSize)
                      .ToList();

        if (containsKeywords)
        {
            // build dictionary<objectId, index>
            var sortMap = hits.Distinct().Select((id, index) => new { Id=id, Index=index }).ToDictionary(p => p.Id, p => p.Index);
            // sort results
            results = results.Select(r => new { Index = sortMap[r.CatalogObjectId], Result = r })
                             .OrderBy(p => p.Index)
                             .Select(p => p.Result)
                             .ToList();
        }

        logger.LogInformation("<<< SearchCatalog end...");

        // return grouped results
        var uiLanguage = request.IsFrench ? CatalogObjectLanguage.French : CatalogObjectLanguage.English;
        return CatalogUtils.GroupResults(results, uiLanguage);
    }

    public async Task<List<CatalogObjectResult>> GetCatalogGroup(Guid groupId)
    {
        using var ctx = contextFactory.CreateDbContext();

        var definitions = await GetLatestMetadataDefinition(ctx);
        var group = await ctx.CatalogObjects
                             .Where(e => e.GroupId == groupId)
                             .Include(e => e.ObjectMetadata)
                             .ThenInclude(s => s.FieldValues)
                             .AsSingleQuery()
                             .Select(c => TransformCatalogObject(c, definitions))
                             .ToListAsync();
        return group;
    }

    private async Task<List<ObjectFieldValue>> CloneMetadataValues(MetadataDbContext ctx, string objectId)
    {
        List<ObjectFieldValue> values = new();

        var metadata = await GetObjectMetadata(ctx, objectId);
        if (metadata is not null)
        {
            values.AddRange(metadata.FieldValues.Select(f => new ObjectFieldValue()
            {
                FieldDefinitionId = f.FieldDefinitionId,
                Value_TXT = f.Value_TXT
            }));
        }

        return values;
    }

    private async Task<Subject> GetSubject(string subjectId)
    {
        using var ctx = contextFactory.CreateDbContext();

        return await ctx.Subjects
            .Include(e => e.SubSubjects)
            .AsSingleQuery()
            .Where(e => e.Subject_TXT == subjectId)
            .FirstOrDefaultAsync();
    }

    private void UpdateRequiresBlanketApproval(ApprovalForm form)
    {
        form.Requires_Blanket_Approval_FLAG = form.Updated_On_Going_Basis_FLAG || form.Collection_Of_Datasets_FLAG || form.Approval_InSitu_FLAG || !string.IsNullOrEmpty(form.Approval_Other_TXT);
        form.Approval_Other_FLAG = !string.IsNullOrEmpty(form.Approval_Other_TXT);
    }

    private async Task<ApprovalForm> GetApprovalFormEntity(MetadataDbContext ctx, int approvalFormId)
    {
        return await ctx.ApprovalForms.FirstOrDefaultAsync(e => e.ApprovalFormId == approvalFormId);
    }

    private async Task<ObjectMetadata> FetchObjectMetadata(MetadataDbContext ctx, string objectId)
    {
        return await ctx.ObjectMetadataSet
                        .Include(e => e.FieldValues)
                        .AsSingleQuery()
                        .FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
    }

    private async Task<ObjectMetadata> CreateNewObjectMetadata(MetadataDbContext ctx, string objectId, int metadataVersionId)
    {
        var objectMetadata = new ObjectMetadata()
        {
            MetadataVersionId = metadataVersionId,
            ObjectId_TXT = objectId,
            FieldValues = new List<ObjectFieldValue>()
        };

        ctx.ObjectMetadataSet.Add(objectMetadata);
        await ctx.TrackSaveChangesAsync(auditingService);

        return objectMetadata;
    }

    private async Task<FieldDefinitions> GetLatestMetadataDefinition(MetadataDbContext ctx)
    {
        var latestVersion = await ctx
            .MetadataVersions
            .Where(e => e.Source_TXT == DefaultSource)
            .OrderByDescending(e => e.Last_Update_DT)
            .FirstOrDefaultAsync();

        if (latestVersion == null)
            return new FieldDefinitions();

        return await GetMetadataDefinition(ctx, latestVersion.MetadataVersionId);
    }

    private async Task<FieldDefinitions> GetMetadataDefinition(MetadataDbContext ctx, int versionId)
    {
        var definitions = new FieldDefinitions();

        var latestDefinitions = await ctx.FieldDefinitions
                .Include(e => e.Choices)
                .AsSingleQuery()
                .Where(e => e.MetadataVersionId == versionId || e.Custom_Field_FLAG)
                .ToListAsync();

        definitions.Add(latestDefinitions);

        return definitions;
    }

    private static IEnumerable<ObjectFieldValue> CloneFieldValues(IEnumerable<ObjectFieldValue> values)
    {
        return values.Select(v => v.Clone());
    }

    public async Task DeleteApprovalForm(int approvalFormId)
    {
        using var ctx = contextFactory.CreateDbContext();
        var approvalForm = await GetApprovalFormEntity(ctx, approvalFormId);
        ctx.ApprovalForms.Remove(approvalForm);
        await ctx.TrackSaveChangesAsync(auditingService);
    }

    public async Task<ObjectMetadata> GetMetadata(long objectMetadataId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.ObjectMetadataSet.FirstOrDefaultAsync(m => m.ObjectMetadataId == objectMetadataId);
    }

    public async Task<ObjectMetadata> GetMetadata(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        return await ctx.ObjectMetadataSet.FirstOrDefaultAsync(m => m.ObjectId_TXT == objectId);
    }

    private static CatalogObjectResult TransformCatalogObject(CatalogObject catObj, FieldDefinitions definitions)
    {
        if (catObj == null)
            return null;

        return new CatalogObjectResult()
        {
            CatalogObjectId = catObj.CatalogObjectId,
            ObjectMetadataId = catObj.ObjectMetadataId,
            MetadataObjectId_TXT = catObj.ObjectMetadata.ObjectId_TXT,
            DataType = catObj.DataType,
            Name_English = catObj.Name_TXT,
            Name_French = catObj.Name_French_TXT,
            Location = catObj.Location_TXT,
            Sector = catObj.Sector_NUM,
            Branch = catObj.Branch_NUM,
            Contact = catObj.Contact_TXT,
            ClassificationType = catObj.Classification_Type,
            Language = catObj.Language,
            Url_English = catObj.Url_English_TXT,
            Url_French = catObj.Url_French_TXT,
            GroupId = catObj.GroupId,
            ProjectId = catObj.ProjectId,
            Metadata = new FieldValueContainer(catObj.ObjectMetadata.ObjectMetadataId, catObj.ObjectMetadata.ObjectId_TXT, definitions,
                catObj.ObjectMetadata.FieldValues)
        };
    }

    public async Task<CatalogObjectResult> GetCatalogObjectByMetadataId(long metadataId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var definitions = await GetLatestMetadataDefinition(ctx);
        var result = await ctx.CatalogObjects.Where(c => c.ObjectMetadataId == metadataId)
            .Include(e => e.ObjectMetadata)
            .ThenInclude(s => s.FieldValues)
            .AsSingleQuery()
            .FirstOrDefaultAsync();

        return await Task.FromResult(TransformCatalogObject(result, definitions));
    }

    public async Task<CatalogObjectResult> GetCatalogObjectByObjectId(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var definitions = await GetLatestMetadataDefinition(ctx);
        var result = await ctx.CatalogObjects.Where(c => c.ObjectMetadata.ObjectId_TXT == objectId)
            .Include(e => e.ObjectMetadata)
            .ThenInclude(s => s.FieldValues)
            .AsSingleQuery()
            .FirstOrDefaultAsync();

        return await Task.FromResult(TransformCatalogObject(result, definitions));
    }

    public async Task<bool> IsObjectCatalogued(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var count = await ctx.CatalogObjects.CountAsync(o => o.ObjectMetadata.ObjectId_TXT == objectId);

        return count > 0;
    }

    public async Task DeleteFromCatalog(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var existingObjects = await ctx.CatalogObjects
            .Where(o => o.ObjectMetadata.ObjectId_TXT == objectId)
            .ToListAsync();

        if (existingObjects?.Count > 0)
        {
            var groupIds = existingObjects.Where(e => e.GroupId.HasValue).Select(e => e.GroupId).ToList();
            foreach (var groupId in groupIds)
            {
                var groupObjects = await ctx.CatalogObjects.Where(o => o.GroupId == groupId).ToListAsync();
                foreach (var catalogObject in groupObjects)
                {
                    catalogObject.GroupId = null;
                }
            }

            foreach (var catalogObject in existingObjects)
            {
                ctx.CatalogObjects.Remove(catalogObject);
            }

            await ctx.TrackSaveChangesAsync(auditingService);
        }
    }

    public async Task DeleteMultipleFromCatalog(IEnumerable<string> objectIds)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();
        using var tran = await ctx.Database.BeginTransactionAsync();

        var existingObjects = await ctx.CatalogObjects
            .Where(o => objectIds.Contains(o.ObjectMetadata.ObjectId_TXT))
            .ToListAsync();

        try
        {
            if (existingObjects?.Count > 0)
            {
                ctx.CatalogObjects.RemoveRange(existingObjects);
                await ctx.TrackSaveChangesAsync(auditingService);
                await tran.CommitAsync();
            }
            else
            {
                await tran.RollbackAsync();
            }
        }
        catch (Exception)
        {
            await tran.RollbackAsync();
        }
    }

    public async Task<Guid> GroupCatalogObjects(IEnumerable<string> objectIds)
    {
        var groupId = Guid.NewGuid();

        using var ctx = await contextFactory.CreateDbContextAsync();

        // collect entries to update
        var updateList = new List<CatalogObject>();
        foreach (var objectId in objectIds)
        {
            // technically there will be one catalog object per object id, but we could suport one-to-many
            var catalogObjects = await ctx.CatalogObjects.Where(e => e.ObjectMetadata.ObjectId_TXT == objectId).ToListAsync();
            updateList.AddRange(catalogObjects);
        }

        // collect groups
        var groupIds = updateList.Where(o => o.GroupId.HasValue).Select(o => o.GroupId).Distinct().ToList();

        // blank groups
        foreach (var id in groupIds)
        {
            var catalogObjects = await ctx.CatalogObjects.Where(e => e.GroupId == id).ToListAsync();
            foreach (var catalogObject in catalogObjects)
            {
                catalogObject.GroupId = null;
            }
        }

        // assign group id
        foreach (var catalogObject in updateList)
        {
            catalogObject.GroupId = groupId;
        }

        await ctx.TrackSaveChangesAsync(auditingService);

        return groupId;
    }

    public async Task<List<string>> GetObjectCatalogGroup(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var metadata = await ctx.ObjectMetadataSet
                                .Include(e => e.CatalogObjects)
                                .AsSingleQuery()
                                .Where(e => e.ObjectId_TXT == objectId)
                                .FirstOrDefaultAsync();

        var catalogGroupId = metadata?.CatalogObjects?.FirstOrDefault()?.GroupId;

        if (catalogGroupId is null)
            return new();

        var groupIds = await ctx.CatalogObjects
                                .Include(e => e.ObjectMetadata)
                                .AsSingleQuery()
                                .Where(e => e.GroupId == catalogGroupId.Value)
                                .Select(e => e.ObjectMetadata.ObjectId_TXT)
                                .ToListAsync();
        return groupIds;
    }

    public async Task<CatalogObjectLanguage?> GetCatalogObjectLanguage(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var catalogObject = await ctx.CatalogObjects.FirstOrDefaultAsync(e => e.ObjectMetadata.ObjectId_TXT == objectId);

        return catalogObject?.Language;
    }

    /// <summary>
    /// GetProjectCatalogItems will only list Files and Power BI reports for now.
    /// </summary>
    /// <param name="projectId">ID of the workspace</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<List<CatalogObjectResult>> GetProjectCatalogItems(int projectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var definitions = await GetLatestMetadataDefinition(ctx);
        List<MetadataObjectType> listedTypes = new() { MetadataObjectType.File, MetadataObjectType.PowerBIReport };

        return await ctx.CatalogObjects
                        .Where(e => e.ProjectId == projectId && listedTypes.Contains(e.DataType))
                        .Include(e => e.ObjectMetadata)
                        .ThenInclude(s => s.FieldValues)
                        .AsSingleQuery()
                        .Select(e => TransformCatalogObject(e, definitions))
                        .ToListAsync();
    }

    public async Task<ClassificationType?> GetObjectClassification(string objectId)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var defId = await GetSecurityClassificationId(ctx);
        var rowValue = await ctx.ObjectFieldValues
            .Include(e => e.ObjectMetadata)
            .AsSingleQuery()
            .Where(e => e.FieldDefinitionId == defId && e.ObjectMetadata.ObjectId_TXT == objectId)
            .Select(e => e.Value_TXT)
            .FirstOrDefaultAsync();

        if (int.TryParse(rowValue, out int value))
            return (ClassificationType)value;

        return null;
    }

    public async Task UpdateMetadata(Stream content)
    {
        try
        {
            var reader = new StreamReader(content);
            var json = await reader.ReadToEndAsync();
            var metadata = JsonSerializer.Deserialize<MetadataDTO>(json);

            await SyncDefinitions(metadata);
            await SyncProfiles(metadata);

            logger.LogInformation($"Test {metadata.Profiles.Count}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync the Metadata");
        }
    }

    public async Task<List<FieldChoice>> GetFieldChoices(string fieldName)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var fieldDefinition = await FindDefinition(ctx, fieldName);
        if (fieldDefinition is null)
            return new();

        return fieldDefinition.Choices.ToList();
    }

    public async Task<List<NameValuePair>> ListObjectFieldValues(string fieldName)
    {
        try
        {
            using var ctx = await contextFactory.CreateDbContextAsync();

            var fieldDefinition = await FindDefinition(ctx, fieldName);
            if (fieldDefinition is null || fieldDefinition.Choices.Count == 0)
                return new();

            var choices = fieldDefinition.Choices.ToDictionary(c => c.Value_TXT, c => c.Label);
            Func<string, string> mapper = c => choices.TryGetValue(c, out string value) ? value : string.Empty;

            var pairs = await ctx.ObjectFieldValues
               .Where(e => e.FieldDefinitionId == fieldDefinition.FieldDefinitionId)
               .Join(ctx.ObjectMetadataSet, e => e.ObjectMetadataId, e => e.ObjectMetadataId, (v, o) => new NameValuePair(o.ObjectId_TXT, v.Value_TXT))
               .ToListAsync();

            return pairs.Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => p with { Value = mapper(p.Value) })?.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);
            throw;
        }
    }

    private static async Task<FieldDefinition> FindDefinition(MetadataDbContext ctx, string fieldName)
    {
        return await ctx.FieldDefinitions
            .Where(f => f.Field_Name_TXT == fieldName)
            .Include(f => f.Choices)
            .AsSingleQuery()
            .FirstOrDefaultAsync();
    }

    private async Task SyncDefinitions(MetadataDTO metadataDto)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var versionId = await CreateVersion(ctx);
        var definitions = await GetLatestMetadataDefinition(ctx);

        foreach (var defDto in metadataDto.Definitions)
        {
            var definition = definitions.Get(defDto.Field_Name_TXT);
            if (definition is null)
            {
                ctx.FieldDefinitions.Add(defDto.ToEntity(versionId));
            }
            else
            {
                var choiceSet = definition.Choices.Select(c => c.Value_TXT).ToHashSet();
                foreach (var choiceDto in defDto.Choices.Where(c => !choiceSet.Contains(c.Value_TXT)))
                {
                    ctx.FieldChoices.Add(choiceDto.ToEntity(definition.FieldDefinitionId));
                }
            }
        }

        await ctx.TrackSaveChangesAsync(auditingService);
    }

    private async Task<int> CreateVersion(MetadataDbContext ctx)
    {
        var version = await ctx.MetadataVersions.FirstOrDefaultAsync(e => e.Source_TXT == DefaultSource);
        if (version is null)
        {
            version = new() { Source_TXT = DefaultSource };
            ctx.MetadataVersions.Add(version);
            await ctx.TrackSaveChangesAsync(auditingService);
        }
        return version.MetadataVersionId;
    }

    private async Task SyncProfiles(MetadataDTO metadataDto)
    {
        using var ctx = await contextFactory.CreateDbContextAsync();

        var definitions = await GetLatestMetadataDefinition(ctx);
        var profiles = await GetProfiles(ctx);

        var profileSet = profiles.Select(p => p.Name).ToHashSet();
        var dtoDefs = metadataDto.Definitions.ToDictionary(d => d.FieldDefinitionId);

        Func<int, int> idMapper = (int id) => definitions.Get(dtoDefs[id].Field_Name_TXT).FieldDefinitionId;

        foreach (var profDto in metadataDto.Profiles.Where(p => !profileSet.Contains(p.Name)))
        {
            var profile = profDto.ToEntity(idMapper);
            ctx.Profiles.Add(profile);
        }

        await ctx.TrackSaveChangesAsync(auditingService);
    }

    private int? securityClassificationId = null;
    private async Task<int> GetSecurityClassificationId(MetadataDbContext ctx)
    {
        if (securityClassificationId is null)
        {
            securityClassificationId = await ctx.FieldDefinitions
                .Where(e => e.Field_Name_TXT == "security_classification")
                .Select(e => e.FieldDefinitionId)
                .FirstOrDefaultAsync();
        }
        return securityClassificationId.Value;
    }

    private async Task<ObjectMetadata> GetObjectMetadata(MetadataDbContext ctx, string objectId)
    {
        return await ctx.ObjectMetadataSet
                        .Include(e => e.FieldValues)
                        .AsSingleQuery()
                        .FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
    }

    private async Task<ObjectMetadata> GetObjectMetadata(MetadataDbContext ctx, long objectMetadataId)
    {
        return await ctx.ObjectMetadataSet
                        .Include(e => e.FieldValues)
                        .AsSingleQuery()
                        .FirstOrDefaultAsync(e => e.ObjectMetadataId == objectMetadataId);
    }

    private void UpdateCatalogIndex(string docId, string title, string content, bool isFrench)
    {
        var catalogSearch = isFrench ? catalogSearchEngine.GetMetadataFrenchSearchEngine() : catalogSearchEngine.GetMetadataEnglishSearchEngine();
        catalogSearch.AddDocument(docId, (title ?? string.Empty).ToLower(), (content ?? string.Empty).ToLower());
        catalogSearch.FlushIndexes();
    }

    private async Task<CatalogObject> GetCatalogObjectCopy(MetadataDbContext ctx, string objectId)
    {
        return await ctx.CatalogObjects.FirstOrDefaultAsync(c => c.ObjectMetadata.ObjectId_TXT == objectId);
    }

    private async Task<bool> CatalogExists(MetadataDbContext ctx, string objectId)
    {
        return await ctx.CatalogObjects.AnyAsync(c => c.ObjectMetadata.ObjectId_TXT == objectId);
    }

    private async Task<List<MetadataProfile>> GetProfiles(MetadataDbContext ctx)
    {
        return await ctx.Profiles
                        .Include(p => p.Sections)
                        .ThenInclude(s => s.Fields)
                        .AsSingleQuery()
                        .ToListAsync();
    }
}