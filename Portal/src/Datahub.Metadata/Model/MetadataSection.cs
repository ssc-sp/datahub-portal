using Datahub.Metadata.Utils;
using System.Collections.Generic;

namespace Datahub.Metadata.Model;

/// <summary>
/// Metadata profile section
/// </summary>
public class MetadataSection
{
    public int SectionId { get; set; }
    public int ProfileId { get; set; }
    public string NameEnglishTXT { get; set; }
    public string NameFrenchTXT { get; set; }
    public virtual MetadataProfile Profile { get; set; }
    public virtual ICollection<SectionField> Fields { get; set; }
    public string Name => CultureUtils.SelectCulture(NameEnglishTXT, NameFrenchTXT);
}