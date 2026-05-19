using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Datahub.Application.Services.Security;
using Datahub.Shared.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Datahub.Shared.Clients;

public class AzureDevOpsClient(AzureDevOpsConfiguration config, ISystemTokenCredentialService tokenCredentialService)
{
    /// <summary>
    /// Represents the Azure DevOps scope used for authentication and authorization.
    /// </summary>
    public const string AzureDevopsScope = "499b84ac-1321-427f-aa17-267ca6975798";

    private static string AzureDevOpsScopeDefault => $"{AzureDevopsScope}/.default";
    private static string AzureManagementApiScopeDefault => "https://management.azure.com/.default";

    public async Task<WorkItemTrackingHttpClient> WorkItemClientAsync()
    {
        var connection = await VssConnectionAsync();
        var client = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

        return client;
    }
    public async Task<HttpClient> PipelineClientAsync()
    {
        var client = new HttpClient();
        var accessToken = await AccessTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token.ToString());
        return client;
    }
    private async Task<VssConnection> VssConnectionAsync()
    {
        var aadCredentials = await Credentials();
        VssConnection vssConnection = new VssConnection(new Uri(config.OrganizationUrl), aadCredentials);
        vssConnection.ConnectAsync().SyncResult();
        return vssConnection;
    }

    private async Task<VssCredentials> Credentials()
    {
        var accessToken = await AccessTokenAsync();
        var aadToken = new VssAadToken("Bearer", accessToken.Token);
        var aadCredentials = new VssAadCredential(aadToken);
        return aadCredentials;
    }
    public async Task<AccessToken> AccessTokenAsync(bool includeAzureManagement = false)
    {
        var tokenCredential = tokenCredentialService.GetPortalTokenCredential() ?? throw new InvalidOperationException("Portal token credential is not available.");
        var scopes = new List<string>
        {
            AzureDevOpsScopeDefault
        };
        if (includeAzureManagement)
        {
            scopes = new List<string>
            {
                AzureManagementApiScopeDefault
            };
        }
        var accessToken =
            await tokenCredential.GetTokenAsync(new TokenRequestContext(scopes.ToArray()), CancellationToken.None);
        return accessToken;
    }
    public AccessToken AccessToken()
    {
        var tokenCredential = tokenCredentialService.GetPortalTokenCredential() ?? throw new InvalidOperationException("Portal token credential is not available.");
        var accessToken =
            tokenCredential.GetToken(new TokenRequestContext([AzureDevOpsScopeDefault]), CancellationToken.None);
        return accessToken;
    }
    public async Task<AccessToken> AccessTokenWithCustomScopeAsync(string customScope)
    {
        var tokenCredential = tokenCredentialService.GetPortalTokenCredential();
        var accessToken =
            await tokenCredential.GetTokenAsync(new TokenRequestContext([customScope]), CancellationToken.None);
        return accessToken;
    }
}
