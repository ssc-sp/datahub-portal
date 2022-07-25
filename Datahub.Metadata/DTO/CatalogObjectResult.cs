using Datahub.Metadata.Model;

namespace Datahub.Metadata.DTO
{
    public class CatalogObjectResult
    {
        public long CatalogObjectId { get; set; }
        public long ObjectMetadataId { get; set; }
        public MetadataObjectType DataType { get; set; }
        public string Name_English { get; set; }
        public string Name_French { get; set; }
        public string Location { get; set; }
        public int Sector { get; set; }
        public int Branch { get; set; }
        public string Contact { get; set; }
        //public string SecurityClass { get; set; }
        public ClassificationType ClassificationType {  get; set; }
        public string Url_English { get; set; }
        public string Url_French { get; set; }
        public CatalogObjectLanguage Language { get; init; }
        public bool IsCatalogComplete { get; set; } = true;
        public bool IsFrench { get; set; }
        public FieldValueContainer Metadata { get; set; }
    }
}
