﻿using System.Collections.Generic;

namespace Datahub.Metadata.Model
{
    public class ObjectMetadata
    {
        /// <summary>
        /// Object metadata PK
        /// </summary>
        public long ObjectMetadataId { get; set; }
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
        /// <summary>
        /// Cataloged objects (because one object can be in more than one subject)
        /// </summary>
        public virtual ICollection<CatalogObject> CatalogObjects { get; set; }
    }
}
