using Microsoft.Extensions.DependencyInjection;

namespace Datahub.GeoCore.Service;

public static class GeoCoreSeviceCollectionExtensions
{
	public static void AddGeoCoreService(this IServiceCollection services)
	{
		services.AddSingleton<IGeoCoreServiceFactory, GeoCoreServiceFactory>();
	}
}