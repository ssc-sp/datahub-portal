using Microsoft.EntityFrameworkCore;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Datahub.Metadata.DTO;
using Entities = Datahub.Metadata.Model;

namespace Datahub.Core.Services
{
    public class MetadataBrokerService : IMetadataBrokerService
    {
        readonly IDbContextFactory<MetadataDbContext> _contextFactory;
        readonly IDatahubAuditingService _auditingService;

        public MetadataBrokerService(IDbContextFactory<MetadataDbContext> contextFactory, IDatahubAuditingService auditingService)
        {
            _contextFactory = contextFactory;
            _auditingService = auditingService;
        }

        public async Task<MetadataProfile> GetProfile(string name)
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await ctx.Profiles
                            .Include(p => p.Sections)
                            .ThenInclude(s => s.Fields)
                            .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<FieldValueContainer> GetObjectMetadataValues(long objectMetadataId, string defaultMetadataId)
        {
            using var ctx = _contextFactory.CreateDbContext();

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
            using var ctx = _contextFactory.CreateDbContext();

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

            using var ctx = _contextFactory.CreateDbContext();

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
                        // add new value
                        editedValue.ObjectMetadataId = current.ObjectMetadataId;
                        ctx.ObjectFieldValues.Add(editedValue);
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
                await ctx.TrackSaveChangesAsync(_auditingService, anonymous);

                transation.Commit();

                return current;
            }
            catch (Exception)
            {
                transation.Rollback();
                throw;
            }
        }

        public async Task<Entities.ApprovalForm> GetApprovalForm(int approvalFormId)
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await GetApprovalFormEntity(ctx, approvalFormId);
        }

        public async Task<int> SaveApprovalForm(ApprovalForm form)
        {
            using var ctx = _contextFactory.CreateDbContext();

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

            await ctx.SaveChangesAsync();

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
                using var ctx = _contextFactory.CreateDbContext();
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
                using var ctx = _contextFactory.CreateDbContext();
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

        public async Task UpdateCatalog(long objectMetadataId, MetadataObjectType dataType, string objectName, string location,
            int sector, int branch, string contact, string securityClass, string englishText, string frenchText)
        {
            using var ctx = _contextFactory.CreateDbContext();

            var transation = ctx.Database.BeginTransaction();
            try
            {
                // delete existing catalog object
                var catalogObjects = await ctx.CatalogObjects.Where(e => e.ObjectMetadataId == objectMetadataId).ToListAsync();
                foreach (var obj in catalogObjects)
                    ctx.CatalogObjects.Remove(obj);

                await ctx.SaveChangesAsync();

                // add new object
                CatalogObject catalogObject = new()
                {
                    ObjectMetadataId = objectMetadataId,
                    DataType = dataType,
                    Name_TXT = objectName,
                    Location_TXT = location,
                    Sector_NUM = sector,
                    Branch_NUM = branch,
                    Contact_TXT = contact,
                    SecurityClass_TXT = !string.IsNullOrEmpty(securityClass) ? securityClass : SecurityClassification.Unclassified,
                    Search_English_TXT = englishText,
                    Search_French_TXT = frenchText
                };

                ctx.CatalogObjects.Add(catalogObject);

                await ctx.SaveChangesAsync();

                transation.Commit();
            }
            catch (Exception)
            {
                transation.Rollback();
                throw;
            }
        }

        public async Task<FieldDefinitions> GetFieldDefinitions()
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await GetLatestMetadataDefinition(ctx);
        }

        public async Task<List<CatalogObjectResult>> SearchCatalog(CatalogSearchRequest request, Func<CatalogObjectResult, bool> validateResult)
        {
            using var ctx = _contextFactory.CreateDbContext();

            var conditions = new List<string>()
            {
                GetSearchTextCondition(request.Keywords, request.IsFrench ? "Search_French_TXT" : "Search_English_TXT"),
                GetOrSearchCondition(request.Classifications, "SecurityClass_TXT"),
                GetOrSearchCondition(request.ObjectTypes.Select(o => (int)o), "DataType"),
                GetOrSearchCondition(request.Sectors, "Sector_NUM"),
                GetOrSearchCondition(request.Branches, "Branch_NUM")
            };
            var whereCondition = string.Join(" AND ", conditions.Where(s => !string.IsNullOrEmpty(s)).Select(s => $"({s})"));

            var query = string.IsNullOrEmpty(whereCondition)
                ? $"SELECT * FROM CatalogObjects WHERE CatalogObjectId > {request.LastPageId}"
                : $"SELECT * FROM CatalogObjects WHERE CatalogObjectId > {request.LastPageId} AND {whereCondition}";

            var definitions = await GetLatestMetadataDefinition(ctx);
            var results = await ctx.QueryCatalog(query);

            return results.Select(e => TransformCatalogObject(e, definitions))
                          .Where(validateResult)
                          .Take(request.PageSize)
                          .ToList();
        }

        private async Task<ObjectMetadata> GetObjectMetadata(MetadataDbContext ctx, string objectId)
        {
            return await ctx.ObjectMetadataSet.Include(e => e.FieldValues).FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
        }

        private async Task<ObjectMetadata> GetObjectMetadata(MetadataDbContext ctx, long objectMetadataId)
        {
            return await ctx.ObjectMetadataSet.Include(e => e.FieldValues).FirstOrDefaultAsync(e => e.ObjectMetadataId == objectMetadataId);
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

        static string GetSearchTextCondition(IEnumerable<string> keywords, string fieldName) => 
            string.Join(" AND ", keywords.Select(kw => $"{fieldName} LIKE '%{string.Concat(PreProcessSearchText(kw))}%'"));

        static string GetOrSearchCondition(IEnumerable<int> values, string fieldName) =>
            string.Join(" OR ", values.Select(v => $"{fieldName} = {v}"));

        static string GetOrSearchCondition(IEnumerable<MetadataClassificationType> values, string fieldName) =>
            string.Join(" OR ", values.Select(v => $"{fieldName} = '{v}'"));

        static IEnumerable<char> PreProcessSearchText(string text)
        {
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c))
                    yield return c;
                else if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
                    yield return ' ';
            }
        }

        private async Task<Subject> GetSubject(string subjectId)
        {
            using var ctx = _contextFactory.CreateDbContext();

            return await ctx.Subjects
                .Include(e => e.SubSubjects)
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
            return await ctx.ObjectMetadataSet.Include(e => e.FieldValues).FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
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
            await ctx.SaveChangesAsync();

            return objectMetadata;
        }

        const string OpenDataId = "OpenData";

        private async Task<FieldDefinitions> GetLatestMetadataDefinition(MetadataDbContext ctx)
        {
            var latestVersion = await ctx
                .MetadataVersions
                .Where(e => e.Source_TXT == OpenDataId)
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
                    .Where(e => e.MetadataVersionId == versionId || e.Custom_Field_FLAG)
                    .ToListAsync();

            definitions.Add(latestDefinitions);

            return definitions;
        }

        static IEnumerable<ObjectFieldValue> CloneFieldValues(IEnumerable<ObjectFieldValue> values)
        {
            return values.Select(v => v.Clone());
        }

        public async Task DeleteApprovalForm(int approvalFormId)
        {
            using var ctx = _contextFactory.CreateDbContext();
            var approvalForm = await GetApprovalFormEntity(ctx, approvalFormId);
            ctx.ApprovalForms.Remove(approvalForm);
            await ctx.SaveChangesAsync();
        }

        public async Task<ObjectMetadata> GetMetadata(long objectMetadataId)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
            return await ctx.ObjectMetadataSet.FirstOrDefaultAsync(m => m.ObjectMetadataId == objectMetadataId);
        }

        public async Task<ObjectMetadata> GetMetadata(string objectId)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();
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
                DataType = catObj.DataType,
                Name = catObj.Name_TXT,
                Location = catObj.Location_TXT,
                Sector = catObj.Sector_NUM,
                Branch = catObj.Branch_NUM,
                Contact = catObj.Contact_TXT,
                SecurityClass = catObj.SecurityClass_TXT,
                Metadata = new FieldValueContainer(catObj.ObjectMetadata.ObjectMetadataId, catObj.ObjectMetadata.ObjectId_TXT, definitions, 
                    catObj.ObjectMetadata.FieldValues)
            };
        } 

        public async Task<CatalogObjectResult> GetCatalogObjectByMetadataId(long metadataId)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            var definitions = await GetLatestMetadataDefinition(ctx);
            var result = await ctx.CatalogObjects.Where(c => c.ObjectMetadataId == metadataId)
                .Include(e => e.ObjectMetadata)
                .ThenInclude(s => s.FieldValues)
                .FirstOrDefaultAsync();

            return await Task.FromResult(TransformCatalogObject(result, definitions));
        }

        public async Task<CatalogObjectResult> GetCatalogObjectByObjectId(string objectId)
        {
            using var ctx = await _contextFactory.CreateDbContextAsync();

            var definitions = await GetLatestMetadataDefinition(ctx);
            var result = await ctx.CatalogObjects.Where(c => c.ObjectMetadata.ObjectId_TXT == objectId)
                .Include(e => e.ObjectMetadata)
                .ThenInclude(s => s.FieldValues)
                .FirstOrDefaultAsync();

            return await Task.FromResult(TransformCatalogObject(result, definitions));
        }
    }
}
