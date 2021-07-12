using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRCan.Datahub.Metadata;
using System.Threading.Tasks;

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
            definitions.Add(await _metadataDbContext.FieldDefinitions.Include(e => e.Choices).ToListAsync());
            return definitions;
        }
    }
}
