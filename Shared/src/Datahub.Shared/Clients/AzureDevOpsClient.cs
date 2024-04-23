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
    private static string AzureDevopsCodeWriteScope => $"{AzureDevopsScope}/vso.code_write";

    public async Task<WorkItemTrackingHttpClient> GetWorkItemClient()
    {
        var connection = await GetVssConnection();
        var client = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        return client;
    }
    public async Task<HttpClient> GetPipelineClient()
    {
        var client = new HttpClient();
        var accessToken = await GetAccessTokenAsync();
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
        var accessToken = await GetAccessTokenAsync();
        var aadToken = new VssAadToken("Bearer", accessToken.Token);
        var aadCredentials = new VssAadCredential(aadToken);
        return aadCredentials;
    }
    public async Task<AccessToken> GetAccessTokenAsync()
    {
        var credentials = new ClientSecretCredential(config.TenantId, config.ClientId,
            config.ClientSecret);
        var accessToken =
            await credentials.GetTokenAsync(new TokenRequestContext([
                AzureDevOpsScopeDefault
            ]));
        return accessToken;
    }
    public AccessToken GetAccessToken()
    {
        var credentials = new ClientSecretCredential(config.TenantId, config.ClientId,
            config.ClientSecret);
        var accessToken =
            credentials.GetToken(new TokenRequestContext([
                AzureDevOpsScopeDefault
            ]));
        return accessToken;
    }
}