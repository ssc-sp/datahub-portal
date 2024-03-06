using System;
using System.Collections.Generic;

namespace Datahub.Metadata.Model;

public class MetadataVersion
{
    public int MetadataVersionId { get; set; }
    public string SourceTXT { get; set; }
    public DateTime LastUpdateDT { get; set; }
    public string VersionInfoTXT { get; set; }
    public virtual ICollection<FieldDefinition> Definitions { get; set; }
    public virtual ICollection<ObjectMetadata> Objects { get; set; }
}