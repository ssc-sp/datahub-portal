using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Datahub.Shared.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Datahub.Shared.Clients;

public class AzureDevOpsClient(AzureDevOpsConfiguration config)
{
    /// <summary>
    /// Represents the Azure DevOps scope used for authentication and authorization.
    /// </summary>
    public const string AzureDevopsScope = "499b84ac-1321-427f-aa17-267ca6975798";

    private static string AzureDevOpsScopeDefault => $"{AzureDevopsScope}/.default";

    public async Task<WorkItemTrackingHttpClient> GetWorkItemClient()
    {
        var connection = await GetVssConnection();
        var client = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        return client;
    }
    public async Task<HttpClient> GetPipelineClient()
    {
        var client = new HttpClient();
        var accessToken = await GetAccessToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token.ToString());
        return client;
    }
    private async Task<VssConnection> GetVssConnection()
    {
        var aadCredentials = await GetCredentials();
        VssConnection vssConnection = new VssConnection(new Uri(config.OrganizationUrl), aadCredentials);
        vssConnection.ConnectAsync().SyncResult();
        return vssConnection;
    }

    private async Task<VssCredentials> GetCredentials()
    {
        var accessToken = await GetAccessToken();
        var aadToken = new VssAadToken("Bearer", accessToken.Token);
        var aadCredentials = new VssAadCredential(aadToken);
        return aadCredentials;
    }
    public async Task<AccessToken> GetAccessToken()
    {
        var credentials = new ClientSecretCredential(config.TenantId, config.ClientId,
            config.ClientSecret);
        var accessToken =
            await credentials.GetTokenAsync(new TokenRequestContext([
                AzureDevOpsScopeDefault
            ]));
        return accessToken;
    }
    public async Task<AccessToken> GetAccessToken(string customScope)
    {
        var credentials = new ClientSecretCredential(config.TenantId, config.ClientId,
            config.ClientSecret);
        var accessToken =
            await credentials.GetTokenAsync(new TokenRequestContext([
                customScope
            ]));
        return accessToken;
    }
}