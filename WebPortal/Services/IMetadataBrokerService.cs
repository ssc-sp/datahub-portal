using NRCan.Datahub.Metadata;
using NRCan.Datahub.Metadata.DTO;
using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services
{
    public interface IMetadataBrokerService
    {
        Task<ObjectMetadataContext> GetMetadataContext(string objectId);
        Task SaveMetadata(string objectId, int metadataVersionId, FieldValueContainer fieldValues);
    }
}
