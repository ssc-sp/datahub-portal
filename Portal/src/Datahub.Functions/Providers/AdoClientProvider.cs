using Azure.Core;
using Azure.Identity;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

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
            var client = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            return client;
        }

        private async Task<VssConnection> GetVssConnection()
        {
            var aadCredentials = await GetCredentials();
            VssConnection vssConnection = new VssConnection(new Uri(_config.AdoConfig.OrgUrl), aadCredentials);
            vssConnection.ConnectAsync().SyncResult();
            return vssConnection;
        }

        private async Task<VssCredentials> GetCredentials()
        {
            const string scope = "499b84ac-1321-427f-aa17-267ca6975798/.default";
            var credentials = new ClientSecretCredential(_config.TenantId, _config.AdoConfig.SpClientId,
                _config.AdoConfig.SpClientSecret);
            var accessToken =
                await credentials.GetTokenAsync(new TokenRequestContext(new[]
                    { scope }));
            var aadToken = new VssAadToken("Bearer", accessToken.Token);
            var aadCredentials = new VssAadCredential(aadToken);
            return aadCredentials;
        }
    }
}