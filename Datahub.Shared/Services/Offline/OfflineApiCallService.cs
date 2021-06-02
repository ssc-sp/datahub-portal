using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
            
    public class OfflineApiCallService : IApiCallService
    {
        public OfflineApiCallService()
        {
        }

        public Task<bool> CallApi(UriBuilder builder, string Url, HttpMethod httpMethod, string secretKey = "DataHub-API-Gateway-Key", Dictionary<string, string> headers = null)
        {
            return Task.FromResult(true);
        }

        public Task<string> CallGetApi(UriBuilder builder, string Url, string secretKey = "DataHub-API-Gateway-Key", Dictionary<string, string> headers = null)
        {
            return Task.FromResult(string.Empty);
        }

        public string getBlobContainerName()
        {
            return "offline-blob-name";
        }

        public Task<string> getStorageConnString()
        {
            return Task.FromResult(string.Empty);
        }
    }
}
