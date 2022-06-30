using Datahub.Metadata.Model;

namespace Datahub.Metadata.DTO
{
    public class CatalogObjectResult
    {
        public long CatalogObjectId { get; set; }
        public long ObjectMetadataId { get; set; }
        public MetadataObjectType DataType { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Sector { get; set; }
        public int Branch { get; set; }
        public string Contact { get; set; }
        public string SecurityClass { get; set; }
        public bool IsCatalogComplete { get; set; } = true;
        public FieldValueContainer Metadata { get; set; }
    }
}
