using System;

namespace Datahub.CKAN.Service;

public class CKANConfiguration
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public bool TestMode { get; set; }

    private Uri BaseUri => new(BaseUrl);
    public string ApiUrl => new Uri(BaseUri, "api").ToString();
    public string DatasetUrl => new Uri(BaseUri, "dataset").ToString();
}