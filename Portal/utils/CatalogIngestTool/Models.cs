namespace CatalogIngestTool;

#nullable disable

public class Entry
{
    public string id { get; set; }
    public string nameEn { get; set; }
    public string nameFr { get; set; }
    public string contact { get; set; }
    public List<Subject> subjects { get; set; }
    public string programs { get; set; }
    public List<string> keywordsEn { get; set; }
    public List<string> keywordsFr { get; set; }
    public string urlEn { get; set; }
    public string urlFr { get; set; }
    public string securityClass { get; set; }
}

public class Subject
{
    public string id { get; set; }
    public string nameEn { get; set; }
    public string nameFr { get; set; }
}

public class Sector
{
    public int Id { get; set; }
    public string NameEnglish { get; set; }
    public string NameFrench { get; set; }
}