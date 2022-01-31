namespace Datahub.Metadata.Model
{
    public class CatalogObject
    {
        public long CatalogObjectId { get; set; }
        public long ObjectMetadataId { get; set; }
        public virtual ObjectMetadata ObjectMetadata { get; set; }
        public MetadataObjectType DataType { get; set; }
        public string Name_TXT { get; set; }
        public string Location_TXT { get; set; }
        public string SecurityClass_TXT { get; set; }
        public int Sector_NUM { get; set; }
        public int Branch_NUM { get; set; }
        public string Contact_TXT { get; set; }
        public string Search_English_TXT { get; set; }
        public string Search_French_TXT { get; set; }
    }
}
