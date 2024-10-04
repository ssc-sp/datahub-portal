using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Datahub.Core.Data.ExternalSearch.OpenData;

public class OpenDataItem
{
    private static readonly Regex BILINGUAL_URL_REGEX = new Regex("u'([^']+)'");

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public BilingualText Title_Translated { get; set; }
    public OpenDataOrganization Organization { get; set; }
    public BilingualText Org_Title_At_Publication { get; set; }
    public BilingualText Notes_Translated { get; set; }
    public string Notes { get; set; }

    public string Url { get; set; }
    private BilingualText _urlDecoded;
    public BilingualText UrlDecoded
    {
        get
        {
            if (_urlDecoded == null && !string.IsNullOrEmpty(Url))
            {
                var replaced = BILINGUAL_URL_REGEX.Replace(Url, "\"$1\"");
                _urlDecoded = JsonConvert.DeserializeObject<BilingualText>(replaced);
            }
            return _urlDecoded;
        }
    }
}