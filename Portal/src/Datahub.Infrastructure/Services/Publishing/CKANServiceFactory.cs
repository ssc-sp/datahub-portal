using Datahub.Application.Configuration;
using Datahub.Application.Services.Publishing;

namespace Datahub.Infrastructure.Services.Publishing;

//TODO remove this class along with the old OpenDataService

public class CKANServiceFactory
{
    readonly DatahubPortalConfiguration _config;
    readonly IHttpClientFactory _httpClientFactory;

    public CKANServiceFactory(IHttpClientFactory httpClientFactory, DatahubPortalConfiguration config)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public ICKANService CreateService() => CreateService(null);
    public ICKANService CreateService(string apiKey) => new CKANService(_httpClientFactory, _config.CkanConfiguration, apiKey);

    public bool IsStaging() => (_config.CkanConfiguration.BaseUrl ?? "").Contains("staging");
}