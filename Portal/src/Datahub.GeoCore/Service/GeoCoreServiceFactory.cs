using Microsoft.Extensions.Options;

namespace Datahub.GeoCore.Service;

public class GeoCoreServiceFactory : IGeoCoreServiceFactory
{
	readonly IOptions<GeoCoreConfiguration> _configuration;
	readonly IHttpClientFactory _httpClientFactory;

	public GeoCoreServiceFactory(IHttpClientFactory httpClientFactory, IOptions<GeoCoreConfiguration> configuration)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
	}

	public IGeoCoreService CreateService() => new GeoCoreService(_httpClientFactory.CreateClient("DatahubApp"), _configuration);
}