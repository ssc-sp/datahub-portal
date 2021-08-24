using System;
using System.Collections.Generic;

namespace NRCan.Datahub.Metadata.Model
{
    public class MetadataVersion
    {
        public int MetadataVersionId { get; set; }
        public string Source_TXT { get; set; }
        public DateTime Last_Update_DT { get; set; }
        public string Version_Info_TXT { get; set; }
        public virtual ICollection<FieldDefinition> Definitions { get; set; }
        public virtual ICollection<ObjectMetadata> Objects { get; set; }
    }
}
