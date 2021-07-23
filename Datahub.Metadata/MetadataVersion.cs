using System;
using System.Collections.Generic;

namespace NRCan.Datahub.Metadata
{
    public class MetadataVersion
    {
        public int Id { get; set; }
        public MetadataSource Source { get; set; }
        public DateTime LastUpdate { get; set; }
        public string VersionData { get; set; }
        public virtual ICollection<FieldDefinition> Definitions { get; set; }
    }
}
