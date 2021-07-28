using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Metadata;
using NRCan.Datahub.Metadata.Model;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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

        public async Task<MetadataDefinition> GetMetadataDefinition(string objectId)
        {
            var objectMetadata = await _metadataDbContext.ObjectMetadataSet.FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
            return await (objectMetadata == null ? GetLatestMetadataDefinition() : GetMetadataDefinition(objectMetadata.MetadataVersionId));
        }

        public async Task<ObjectMetadataContext> GetMetadataContext(string objectId)
        {
            var objectMetadata = await _metadataDbContext.ObjectMetadataSet.Include(e => e.FieldValues).FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);

            var metadataDefinition = await (objectMetadata == null ? GetLatestMetadataDefinition() : GetMetadataDefinition(objectMetadata.MetadataVersionId));

            var fieldValues = objectMetadata?.FieldValues ?? new List<ObjectFieldValue>();

            return new ObjectMetadataContext(objectId, metadataDefinition, fieldValues);
        }

        public async Task SaveMetadata(string objectId, IEnumerable<ObjectFieldValue> fieldValues)
        {
            var current = await FetchObjectMetadata(objectId) ?? CreateNewObjectMetadata(objectId);

            if (current.ObjectMetadataId == 0)
            {
                await _metadataDbContext.SaveChangesAsync();
            }

            var newValues = fieldValues.ToDictionary(v => v.FieldDefinitionId, v => v);
            var currentValues = current.FieldValues.ToDictionary(v => v.FieldDefinitionId, v => v);

            foreach (var fv in fieldValues)
            {
                if (currentValues.TryGetValue(fv.FieldDefinitionId, out ObjectFieldValue value))
                {
                    value.Value_TXT = fv.Value_TXT;
                    _metadataDbContext.ObjectFieldValues.Update(fv);
                }
                else
                {
                    fv.ObjectMetadataId = current.ObjectMetadataId;
                    _metadataDbContext.ObjectFieldValues.Add(fv);
                }
            }


            // for each existing value 


            throw new System.NotFiniteNumberException();
        }

        private async Task<ObjectMetadata> FetchObjectMetadata(string objectId)
        {
            return await _metadataDbContext.ObjectMetadataSet.FirstOrDefaultAsync(e => e.ObjectId_TXT == objectId);
        }

        private ObjectMetadata CreateNewObjectMetadata(string objectId)
        {
            return new ObjectMetadata()
            {
                ObjectId_TXT = objectId,
                FieldValues = new List<ObjectFieldValue>()
            };
        }

        const string OpenDataId = "OpenData";

        private async Task<MetadataDefinition> GetLatestMetadataDefinition()
        {
            var latestVersion = await _metadataDbContext
                .MetadataVersions
                .Where(e => e.Source_TXT == OpenDataId)
                .OrderByDescending(e => e.Last_Update_DT)
                .FirstOrDefaultAsync();

            if (latestVersion == null)
                return new MetadataDefinition();

            return await GetMetadataDefinition(latestVersion.MetadataVersionId);
        }

        private async Task<MetadataDefinition> GetMetadataDefinition(int versionId)
        {
            var definitions = new MetadataDefinition();

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
    }
}
