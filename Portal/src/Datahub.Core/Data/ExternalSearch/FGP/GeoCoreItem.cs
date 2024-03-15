using Newtonsoft.Json;

namespace Datahub.Core.Data.ExternalSearch.FGP;

public class GeoCoreItem
{
    public Guid Id { get; set; }
    public string Published { get; set; } // TODO potentially incomplete dates (yyyy-mm or yyyy)
    public string Organisation { get; set; }
    public string Type { get; set; }
    public string TopicCategory { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; } // TODO turn it into a list
    public string SpatialRepresentation { get; set; }
    public string Language { get; set; } // json source: "eng; CAN" -> maybe turn it into a CultureInfo
    public string TemporalExtent { get; set; } // TODO some processing? (pair of month dates)
    public string Coordinates { get; set; } // TODO some processing (list of points)
    public string Created { get; set; } // TODO some processing (month date)

    public string Options { get; set; } // TODO lots of processing (JSON w/ multiple escape, multiple quotes)
    public string GraphicOverview { get; set; } // TODO lots of processing (same)
    public string Contact { get; set; } // TODO lots of processing (same)

    private GeoCoreOptionsList _optionsList = null;
    private GeoCoreContactList _contactList = null;
    private GeoCoreGraphicOverviewList _graphicsList = null;

    public GeoCoreOptionsList OptionsList
    {
        get
        {
            if (_optionsList == null)
            {
                _optionsList = DecodeEscapedJson<GeoCoreOptionsList>(Options);
            }
            return _optionsList;
        }
    }

    public GeoCoreContactList ContactList
    {
        get
        {
            if (_contactList == null)
            {
                _contactList = DecodeEscapedJson<GeoCoreContactList>(Contact);
            }
            return _contactList;
        }
    }

    public GeoCoreContact FirstContact => ContactList.Count > 0 ? ContactList[0] : null;

    public GeoCoreGraphicOverviewList GraphicOverviewList
    {
        get
        {
            if (_graphicsList == null)
            {
                _graphicsList = DecodeEscapedJson<GeoCoreGraphicOverviewList>(GraphicOverview);
            }
            return _graphicsList;
        }
    }

    public GeoCoreGraphicOverview FirstGraphicOverview => GraphicOverviewList.Count > 0 ? GraphicOverviewList[0] : null;

    private T DecodeEscapedJson<T>(string content)
    {
        var decoded = content.Replace("\\\"\"", "\"").Replace("\"\"", string.Empty);
        var result = JsonConvert.DeserializeObject<T>(decoded);
        return result;
    }

    public string GetGeoCaUrl(string lang = "en")
    {
        return $"https://app.geo.ca/result?id={Id}&lang={lang}";
    }

    /*
"id": "000183ed-8864-42f0-ae43-c4313a860720",
"published": "2020-02-27",
"organisation": "Government of Canada; Natural Resources Canada; Lands and Minerals Sector",
"type": "series; s\u00e9rie",
"topicCategory": "economy",
"title": "Principal Mineral Areas, Producing Mines, and Oil and Gas Fields (900A)",
"description": "[bunch of words]",
"keywords": "[bunch of words, comma separated]",
"spatialRepresentation": "vector; vecteur",
"language": "eng; CAN",
"temporalExtent": "{begin=2020-01, end=2020-12}",
"coordinates": "[[[-141.003, 41.6755], [-52.6174, 41.6755], [-52.6174, 83.1139], [-141.003, 83.1139], [-141.003, 41.6755]]]",
"created": "2021-02"

"contact": "[json string]",
"options": "[json string]",
"graphicOverview": "[json string]",

"total": "1",
"row_num": "1",
    */
}