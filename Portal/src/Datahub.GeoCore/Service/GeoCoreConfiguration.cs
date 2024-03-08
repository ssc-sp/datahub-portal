namespace Datahub.GeoCore.Service;

public class GeoCoreConfiguration
{
	public string? BaseUrl { get; set; }
	public string? ApiKey { get; set; }
	public int SourceKey { get; set; }
	public string? DatasetBaseUrl { get; set; }
	public bool TestMode { get; set; }
}