using System.Collections.Generic;

namespace NRCan.Datahub.Metadata.Model
{
    public class ObjectMetadata
    {
        /// <summary>
        /// Object metadata PK
        /// </summary>
        public ulong ObjectMetadataId { get; set; }
        /// <summary>
        /// Metadata Version FK
        /// </summary>
        public int MetadataVersionId { get; set; }
        /// <summary>
        /// External object identifier (restricted to 128 chars)
        /// </summary>
        public string ObjectId_TXT { get; set; }
        /// <summary>
        /// Parent metadata version
        /// </summary>
        public virtual MetadataVersion MetadataVersion { get; set; }
        /// <summary>
        /// Field values collection
        /// </summary>
        public virtual ICollection<ObjectFieldValue> FieldValues { get; set; }
    }
}
