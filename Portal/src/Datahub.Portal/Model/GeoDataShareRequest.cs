namespace Datahub.Portal.Model;

public class GeoDataShareRequest
{
    public string titleEn { get; set; }
    public string titleFr { get; set; }
    public string keywordsEn { get; set; }
    public string keywordsFr { get; set; }
    public List<GeoDataContact> contact { get; set; }
}

public class GeoDataContact
{
    public GeoDataContactEmail email { get; set; }
}

public class GeoDataContactEmail
{
    public string en { get; set; }
    public string fr { get; set; }
}