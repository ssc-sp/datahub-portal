using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Datahub.Shared.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Datahub.Shared.Clients;

public interface IAzureDevopsConfiguration
{
    string OrganizationUrl { get; }
}

public class AzureDevOpsClient(IAzureDevopsConfiguration config, AzAccessTokenManager tokenManager)
{
    public async Task<WorkItemTrackingHttpClient> WorkItemClientAsync()
    {
        var connection = await VssConnectionAsync();
        return await connection.GetClientAsync<WorkItemTrackingHttpClient>();
    }

    public async Task<HttpClient> PipelineClientAsync()
    {
        var client = new HttpClient();
        var accessToken = await tokenManager.AccessDevopsTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token.ToString());
        return client;
    }

    private async Task<VssConnection> VssConnectionAsync()
    {
        var aadCredentials = await Credentials();
        var vssConnection = new VssConnection(new Uri(config.OrganizationUrl), aadCredentials);
        vssConnection.ConnectAsync().SyncResult();
        return vssConnection;
    }

    private async Task<VssCredentials> Credentials()
    {
        var accessToken = await tokenManager.AccessDevopsTokenAsync();
        var aadToken = new VssAadToken("Bearer", accessToken.Token);
        return new VssAadCredential(aadToken);
    }
}
