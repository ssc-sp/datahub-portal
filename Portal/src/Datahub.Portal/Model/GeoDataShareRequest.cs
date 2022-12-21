namespace Datahub.Portal.Model;

public class GeoDataShareRequest
{
    public string title_en { get; set; }
    public string title_fr { get; set; }
    public string keywords_en { get; set; }
    public string keywords_fr { get; set; }
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