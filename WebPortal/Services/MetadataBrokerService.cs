using Microsoft.EntityFrameworkCore;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Datahub.Metadata.DTO;
using ShareWorkflow = Datahub.Portal.Data.Forms.ShareWorkflow;
using Datahub.Core.Utils;
using Datahub.Core.Services;
using Microsoft.Data.SqlClient;

namespace Datahub.Portal.Services
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

        public async Task<FieldValueContainer> GetObjectMetadataValues(long objectMetadataId)
        {
            using var ctx = _contextFactory.CreateDbContext();

            // retrieve the object metadata
            var objectMetadata = await ctx.ObjectMetadataSet
                .Include(e => e.FieldValues)
                .FirstOrDefaultAsync(e => e.ObjectMetadataId == objectMetadataId);

            // retrieve the field definitions
            var metadataDefinitions = await (objectMetadata == null ? GetLatestMetadataDefinition(ctx) : GetMetadataDefinition(ctx, objectMetadata.MetadataVersionId));

            // retrieve and clone the field values
            var fieldValues = CloneFieldValues(objectMetadata?.FieldValues ?? new List<ObjectFieldValue>());

            return new FieldValueContainer(objectMetadata?.ObjectMetadataId ?? 0, objectMetadata.ObjectId_TXT, metadataDefinitions, fieldValues);
        }

        public async Task<FieldValueContainer> GetObjectMetadataValues(string objectId)
        {
            using var ctx = _contextFactory.CreateDbContext();

            // retrieve the object metadata
            var objectMetadata = await ctx.ObjectMetadataSet
                .Include(e => e.FieldValues)
                .FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);

            // retrieve the field definitions
            var metadataDefinitions = await (objectMetadata == null ? GetLatestMetadataDefinition(ctx) : GetMetadataDefinition(ctx, objectMetadata.MetadataVersionId));

            // retrieve and clone the field values
            var fieldValues = CloneFieldValues(objectMetadata?.FieldValues ?? new List<ObjectFieldValue>());

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

        public async Task<ShareWorkflow.ApprovalForm> GetApprovalForm(int approvalFormId)
        {
            using var ctx = _contextFactory.CreateDbContext();
            
            var approvalFormEntity = await GetApprovalFormEntity(ctx, approvalFormId);
            if (approvalFormEntity != null)
            {
                var approvalForm = new ShareWorkflow.ApprovalForm();
                approvalFormEntity.CopyPublicPropertiesTo(approvalForm, true);
                return approvalForm;
            }

            return null;
        }

        public async Task<int> SaveApprovalForm(ShareWorkflow.ApprovalForm form)
        {
            using var ctx = _contextFactory.CreateDbContext();

            var approvalFormEntity = new ApprovalForm();
            form.CopyPublicPropertiesTo(approvalFormEntity, true);

            // this is calculated field to simplify the PBI PDF generation
            UpdateRequiresBlanketApproval(approvalFormEntity);

            if (approvalFormEntity.ApprovalFormId == 0)
            {
                ctx.ApprovalForms.Add(approvalFormEntity);
            }
            else
            {
                ctx.Attach(approvalFormEntity);
                ctx.ApprovalForms.Update(approvalFormEntity);
            }

            await ctx.SaveChangesAsync();

            return approvalFormEntity.ApprovalFormId;
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

        const string KeywordSeparator = "|";

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
                    SecurityClass_TXT = !string.IsNullOrEmpty(securityClass) ? securityClass : "Unclasified",
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

        public async Task<List<CatalogObjectResult>> SearchCatalogEnglish(string searchText)
        {
            return await SearchCatalog(searchText, "Search_English_TXT");
        }

        public async Task<List<CatalogObjectResult>> SearchCatalogFrench(string searchText)
        {
            return await SearchCatalog(searchText, "Search_French_TXT");
        }

        public async Task<FieldDefinitions> GetFieldDefinitions()
        {
            using var ctx = _contextFactory.CreateDbContext();
            return await GetLatestMetadataDefinition(ctx);
        }

        private async Task<List<CatalogObjectResult>> SearchCatalog(string searchText, string fieldName)
        {
            using var ctx = _contextFactory.CreateDbContext();
            
            var query = PrepareCatalogSearchQuery(searchText, fieldName);
            if (string.IsNullOrEmpty(query))
                return new();

            var results = await ctx.QueryCatalog(query);

            return results.Select(r => new CatalogObjectResult
            (
                r.ObjectMetadataId, 
                r.DataType, 
                r.Name_TXT, 
                r.Location_TXT,
                r.Sector_NUM, 
                r.Branch_NUM, 
                r.Contact_TXT,
                r.SecurityClass_TXT
            )).ToList();
        }

        static string PrepareCatalogSearchQuery(string searchText, string fieldName)
        {
            var filteredSearchText = string
                .Concat(PreProcessSearchText(searchText))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => $"{fieldName} LIKE '%{word}%'");

            var whereCondition = string.Join(" AND ", filteredSearchText);
            if (string.IsNullOrEmpty(whereCondition))
                return "SELECT * FROM CatalogObjects";

            return $"SELECT * FROM CatalogObjects WHERE {whereCondition}";
        }

        static IEnumerable<char> PreProcessSearchText(string text)
        {
            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c))
                    yield return c;
                else if (char.IsWhiteSpace(c))
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
            var objectMetadata =  new ObjectMetadata()
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

        public Task CatalogObject()
        {
            // ...
            return Task.CompletedTask;
        }
    }
}
