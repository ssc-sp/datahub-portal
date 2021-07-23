using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Metadata;
using NRCan.Datahub.Metadata.Model;
using System.Threading.Tasks;
using System.Linq;

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

        public async Task<MetadataDefinition> GetMetadataDefinition()
        {
            var definitions = new MetadataDefinition();

            var latestVersion = await _metadataDbContext
                .MetadataVersions
                .Where(e => e.Source_TXT == "OpenData")
                .OrderByDescending(e => e.Last_Update_DT)
                .FirstOrDefaultAsync();

            if (latestVersion != null)
            {
                var latestDefinitions = await _metadataDbContext.FieldDefinitions
                    .Include(e => e.Choices)
                    .Where(e => e.MetadataVersionId == latestVersion.MetadataVersionId)
                    .ToListAsync();

                definitions.Add(latestDefinitions.Where(IsValidDefinition));
            }

            return definitions;
        }

        static bool IsValidDefinition(FieldDefinition field)
        {
            return !string.IsNullOrWhiteSpace(field.Field_Name_TXT) && !string.IsNullOrWhiteSpace(field.Name_English_TXT);
        }
    }
}
