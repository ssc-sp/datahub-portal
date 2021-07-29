using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Metadata.Model;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using NRCan.Datahub.Metadata.DTO;

namespace NRCan.Datahub.Portal.Services
{
    public class MetadataBrokerService : IMetadataBrokerService
    {
        readonly ILogger<MetadataBrokerService> _logger;
        readonly MetadataDbContext _metadataDbContext;

        public MetadataBrokerService(MetadataDbContext metadataDbContext, ILogger<MetadataBrokerService> logger)
        {
            _logger = logger;
            _metadataDbContext = metadataDbContext;
        }

        public async Task<ObjectMetadataContext> GetMetadataContext(string objectId)
        {
            // retrieve the object metadata
            var objectMetadata = await _metadataDbContext.ObjectMetadataSet
                .Include(e => e.FieldValues)
                .FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);

            // retrieve the field definitions
            var metadataDefinitions = await (objectMetadata == null ? GetLatestMetadataDefinition() : GetMetadataDefinition(objectMetadata.MetadataVersionId));

            // retrieve and clone the field values
            var fieldValues = CloneFieldValues(objectMetadata?.FieldValues ?? new List<ObjectFieldValue>());

            return new ObjectMetadataContext(objectId, metadataDefinitions, fieldValues);
        }

        public async Task SaveMetadata(string objectId, int metadataVersionId, FieldValueContainer fieldValues)
        {
            var transation = _metadataDbContext.Database.BeginTransaction();
            try
            {
                // fetch the existing metadata object or create a new one
                var current = await FetchObjectMetadata(objectId) ?? await CreateNewObjectMetadata(objectId, metadataVersionId);
                
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
                                _metadataDbContext.ObjectFieldValues.Remove(currentValue);
                            }
                            else
                            {
                                // update value
                                currentValue.Value_TXT = editedValue.Value_TXT;
                                _metadataDbContext.ObjectFieldValues.Update(currentValue);
                            }
                        }
                    }
                    else
                    {
                        // add new value
                        editedValue.ObjectMetadataId = current.ObjectMetadataId;
                        _metadataDbContext.ObjectFieldValues.Add(editedValue);
                    }
                }

                // delete removed values
                foreach (var currentValue in current.FieldValues)
                {
                    if (!newValues.ContainsKey(currentValue.FieldDefinitionId))
                    {
                        _metadataDbContext.ObjectFieldValues.Remove(currentValue);
                    }
                }

                // save the changes
                await _metadataDbContext.SaveChangesAsync();

                transation.Commit();
            }
            catch (Exception)
            {
                transation.Rollback();
                throw;
            }
        }

        private async Task<ObjectMetadata> FetchObjectMetadata(string objectId)
        {
            return await _metadataDbContext.ObjectMetadataSet.FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
        }

        private async Task<ObjectMetadata> CreateNewObjectMetadata(string objectId, int metadataVersionId)
        {
            var objectMetadata =  new ObjectMetadata()
            {
                MetadataVersionId = metadataVersionId,
                ObjectId_TXT = objectId,
                FieldValues = new List<ObjectFieldValue>()
            };

            _metadataDbContext.ObjectMetadataSet.Add(objectMetadata);
            await _metadataDbContext.SaveChangesAsync();

            return objectMetadata;
        }

        const string OpenDataId = "OpenData";

        private async Task<FieldDefinitions> GetLatestMetadataDefinition()
        {
            var latestVersion = await _metadataDbContext
                .MetadataVersions
                .Where(e => e.Source_TXT == OpenDataId)
                .OrderByDescending(e => e.Last_Update_DT)
                .FirstOrDefaultAsync();

            if (latestVersion == null)
                return new FieldDefinitions();

            return await GetMetadataDefinition(latestVersion.MetadataVersionId);
        }

        private async Task<FieldDefinitions> GetMetadataDefinition(int versionId)
        {
            var definitions = new FieldDefinitions();

            var latestDefinitions = await _metadataDbContext.FieldDefinitions
                    .Include(e => e.Choices)
                    .Where(e => e.MetadataVersionId == versionId)
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
