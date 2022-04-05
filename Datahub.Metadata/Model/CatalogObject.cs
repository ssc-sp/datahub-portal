namespace Datahub.Metadata.Model
{
    public class CatalogObject
    {
        public long CatalogObjectId { get; set; }
        public long ObjectMetadataId { get; set; }
        public virtual ObjectMetadata ObjectMetadata { get; set; }
        public MetadataObjectType DataType { get; set; }
        /// <summary>
        /// Name or title of the object
        /// </summary>
        public string Name_TXT { get; set; }
        /// <summary>
        /// Location of the object (path, url, key, id, etc)
        /// </summary>
        public string Location_TXT { get; set; }
        /// <summary>
        /// Unclassified, Protect A, Protect B
        /// </summary>
        public string SecurityClass_TXT { get; set; }
        /// <summary>
        /// Sector number
        /// </summary>
        public int Sector_NUM { get; set; }
        /// <summary>
        /// Branch number
        /// </summary>
        public int Branch_NUM { get; set; }
        /// <summary>
        /// Email, name or any way of contact with the cataloged object
        /// </summary>
        public string Contact_TXT { get; set; }
        /// <summary>
        /// Search text English
        /// </summary>
        public string Search_English_TXT { get; set; }
        /// <summary>
        /// Search text French
        /// </summary>
        public string Search_French_TXT { get; set; }
    }
}
