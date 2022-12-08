using Datahub.Metadata.Utils;
using System.Collections.Generic;

namespace Datahub.Metadata.Model
{
    /// <summary>
    /// Metadata profile section
    /// </summary>
    public class MetadataSection
    {
        public int SectionId { get; set; }
        public int ProfileId { get; set; }
        public string Name_English_TXT { get; set; }
        public string Name_French_TXT { get; set; }
        public virtual MetadataProfile Profile { get; set; }
        public virtual ICollection<SectionField> Fields { get; set; }
        public string Name => CultureUtils.SelectCulture(Name_English_TXT, Name_French_TXT);
    }
}
