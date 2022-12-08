using System.Collections.Generic;

namespace Datahub.Metadata.Model
{
    /// <summary>
    /// Metadata profile
    /// </summary>
    public class MetadataProfile
    {
        public int ProfileId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<MetadataSection> Sections { get; set; }
    }
}
