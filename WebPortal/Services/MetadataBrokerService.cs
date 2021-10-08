using Microsoft.EntityFrameworkCore;
using Datahub.Metadata.Model;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using Datahub.Metadata.DTO;
using ShareWorkflow = Datahub.Portal.Data.Forms.ShareWorkflow;
using Datahub.Core.Utils;
using Datahub.Core.Services;

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

        public async Task<ObjectMetadataContext> GetMetadataContext(string objectId)
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

            return new ObjectMetadataContext(objectId, metadataDefinitions, fieldValues);
        }

        public async Task SaveMetadata(string objectId, int metadataVersionId, FieldValueContainer fieldValues)
        {
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
                await ctx.TrackSaveChangesAsync(_auditingService);

                transation.Commit();
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

            definitions.Add(latestDefinitions.Where(IsValidDefinition));

            return definitions;
        }

        static bool IsValidDefinition(FieldDefinition field)
        {
            return !string.IsNullOrWhiteSpace(field.Field_Name_TXT) && !string.IsNullOrWhiteSpace(field.Name_English_TXT);
        }

        static IEnumerable<ObjectFieldValue> CloneFieldValues(IEnumerable<ObjectFieldValue> values)
        {
            return values.Select(v => v.Clone());
        }
    }
}
