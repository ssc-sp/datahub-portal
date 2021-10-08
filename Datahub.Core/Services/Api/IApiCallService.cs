using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Datahub.Shared.Services
{
    public interface IApiCallService
    {
        Task<bool> CallApi(UriBuilder builder, string Url, HttpMethod httpMethod, string secretKey = "DataHub-API-Gateway-Key", Dictionary<string, string> headers = null);
        Task<string> CallGetApi(UriBuilder builder, string Url, string secretKey = "DataHub-API-Gateway-Key", Dictionary<string, string> headers = null);
        string getBlobContainerName();
        Task<string> getStorageConnString();
        Task<string> GetProjectConnectionString(string accountName);
    }
}