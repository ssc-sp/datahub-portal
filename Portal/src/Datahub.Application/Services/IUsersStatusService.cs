using Datahub.Application.Configuration;

namespace Datahub.Application.Services
{
    public interface IUsersStatusService
    {
        public Task<Dictionary<string, List<string>>> GetUsersStatus(IHttpClientFactory _httpClientFactory, DatahubPortalConfiguration _datahubPortalConfiguration);
    }
}