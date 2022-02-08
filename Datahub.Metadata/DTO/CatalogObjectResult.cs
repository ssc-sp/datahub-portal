using Datahub.Metadata.Model;

namespace Datahub.Metadata.DTO
{
    public record CatalogObjectResult
    (
        long ObjectMetadataId,
        MetadataObjectType DataType,
        string Name,
        string Location,
        int Sector,
        int Branch,
        string Contact,
        string SecurityClass
    );
}
