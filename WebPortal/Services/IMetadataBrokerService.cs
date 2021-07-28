using NRCan.Datahub.Metadata;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services
{
    public interface IMetadataBrokerService
    {
        Task<MetadataDefinition> GetMetadataDefinition(string objectId);
        Task<ObjectMetadataContext> GetMetadataContext(string objectId);
    }
}
