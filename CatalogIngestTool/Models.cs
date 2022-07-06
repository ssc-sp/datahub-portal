namespace CatalogIngestTool;

#nullable disable

public class Entry
{
    public string id { get; set; }
    public string name_en { get; set; }
    public string name_fr { get; set; }
    public string contact { get; set; }
    public List<Subject> subjects { get; set; }
    public string programs { get; set; }
    public List<string> keywords_en { get; set; }
    public List<string> keywords_fr { get; set; }
    public string url_en { get; set; }
    public string url_fr { get; set; }
    public string securityClass { get; set; }
}

public class Subject
{
    public string id { get; set; }
    public string name_en { get; set; }
    public string name_fr { get; set; }
}

public class Sector
{
    public int Id { get; set; }
    public string Name_English { get; set; }
    public string Name_French { get; set; }
}