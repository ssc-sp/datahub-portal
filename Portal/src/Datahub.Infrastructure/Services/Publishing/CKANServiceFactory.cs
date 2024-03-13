using Datahub.Application.Configuration;
using Datahub.Application.Services.Publishing;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Datahub.Infrastructure.Services.Publishing;

public class CKANServiceFactory : ICKANServiceFactory
{
    readonly DatahubPortalConfiguration _config;
    readonly IHttpClientFactory _httpClientFactory;

    public CKANServiceFactory(IHttpClientFactory httpClientFactory, DatahubPortalConfiguration config)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public ICKANService CreateService() => CreateService(null);
    public ICKANService CreateService(string apiKey) => new CKANService(_httpClientFactory.CreateClient("CkanClient"), _config.CKAN, apiKey);

    public bool IsStaging() => (_config.CKAN.BaseUrl ?? "").Contains("staging");
}