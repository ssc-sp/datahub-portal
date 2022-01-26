namespace Datahub.Metadata.Model
{
    public class CatalogObject
    {
        public long CatalogObjectId { get; set; }
        public long ObjectMetadataId { get; set; }
        public virtual ObjectMetadata ObjectMetadata { get; set; }
        public string Name_TXT { get; set; }
        public string Search_English_TXT { get; set; }
        public string Search_French_TXT { get; set; }
    }
}
