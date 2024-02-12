using System.Net.Http.Headers;
using System.Text;
using Azure.Core;
using Azure.Identity;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace Datahub.Functions.Providers
{
    public class AdoClientProvider
    {
        private readonly AzureConfig _config;

        public AdoClientProvider(AzureConfig config)
        {
            _config = config;
        }

        public async Task<WorkItemTrackingHttpClient> GetWorkItemClient()
        {
            var connection = await GetVssConnection();
            var client = connection.GetClient<WorkItemTrackingHttpClient>();

            return client;
        }
        private async Task<VssConnection> GetVssConnection()
        {
            var credentials = new ClientSecretCredential(_config.TenantId, _config.AdoConfig.SpClientId,
                _config.AdoConfig.SpClientSecret);
            var accessToken =
                await credentials.GetTokenAsync(new TokenRequestContext(new[]
                    { _config.ClientId + "/.default" }));
            var aadToken = new VssAadToken("Bearer", accessToken.Token);
            var aadCredentials = new VssAadCredential(aadToken);
            VssConnection vssConnection = new VssConnection(new Uri(_config.AdoConfig.OrgUrl), aadCredentials);
            return vssConnection;
        }
    }
}