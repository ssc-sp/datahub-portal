using System.Collections.Generic;

namespace Datahub.Metadata.Catalog;

public class CatalogEntry
{
    public string id { get; set; }
    public string nameEn { get; set; }
    public string nameFr { get; set; }
    public string descEn { get; set; }
    public string descFr { get; set; }
    public string contact { get; set; }
    public List<CatalogEntrySubject> subjects { get; set; }
    public string programs { get; set; }
    public List<string> keywordsEn { get; set; }
    public List<string> keywordsFr { get; set; }
    public string urlEn { get; set; }
    public string urlFr { get; set; }
    public string classification { get; set; }
}

public class CatalogEntrySubject
{
    public string id { get; set; }
    public string nameEn { get; set; }
    public string nameFr { get; set; }
}

public class CatalogEntrySector
{
    public int Id { get; set; }
    public string NameEnglish { get; set; }
    public string NameFrench { get; set; }
}