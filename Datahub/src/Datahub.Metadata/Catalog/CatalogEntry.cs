using System.Collections.Generic;

namespace Datahub.Metadata.Catalog;

public class CatalogEntry
{
    public string id { get; set; }
    public string name_en { get; set; }
    public string name_fr { get; set; }
    public string desc_en { get; set; }
    public string desc_fr { get; set; }
    public string contact { get; set; }
    public List<CatalogEntrySubject> subjects { get; set; }
    public string programs { get; set; }
    public List<string> keywords_en { get; set; }
    public List<string> keywords_fr { get; set; }
    public string url_en { get; set; }
    public string url_fr { get; set; }
    public string classification { get; set; }
}

public class CatalogEntrySubject
{
    public string id { get; set; }
    public string name_en { get; set; }
    public string name_fr { get; set; }
}

public class CatalogEntrySector
{
    public int Id { get; set; }
    public string Name_English { get; set; }
    public string Name_French { get; set; }
}