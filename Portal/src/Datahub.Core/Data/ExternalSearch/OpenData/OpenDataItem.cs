using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Datahub.Core.Data.ExternalSearch.OpenData;

public class OpenDataItem
{
    private static readonly Regex BILINGUAL_URL_REGEX = new Regex("u'([^']+)'");

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public BilingualText TitleTranslated { get; set; }
    public OpenDataOrganization Organization { get; set; }
    public BilingualText OrgTitleAtPublication { get; set; }
    public BilingualText NotesTranslated { get; set; }
    public string Notes { get; set; }

    public string Url { get; set; }
    private BilingualText urlDecoded;
    public BilingualText UrlDecoded
    {
        get
        {
            if (urlDecoded == null && !string.IsNullOrEmpty(Url))
            {
                var replaced = BILINGUAL_URL_REGEX.Replace(Url, "\"$1\"");
                urlDecoded = JsonConvert.DeserializeObject<BilingualText>(replaced);
            }
            return urlDecoded;
        }
    }
}