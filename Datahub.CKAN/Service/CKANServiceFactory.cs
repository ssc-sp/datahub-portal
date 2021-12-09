using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Datahub.CKAN.Service
{
    public class CKANServiceFactory : ICKANServiceFactory
    {
        readonly IOptions<CKANConfiguration> _ckanConfiguration;
        readonly IHttpClientFactory _httpClientFactory;

        public CKANServiceFactory(IHttpClientFactory httpClientFactory, IOptions<CKANConfiguration> ckanConfiguration)
        {
            _ckanConfiguration = ckanConfiguration;
            _httpClientFactory = httpClientFactory;
        }

        public ICKANService CreateService() => new CKANService(_httpClientFactory.CreateClient(), _ckanConfiguration);

        public bool IsStaging() => (_ckanConfiguration.Value.BaseUrl ?? "").Contains("staging");
    }
}
