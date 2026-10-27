using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Datahub.Shared.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Datahub.Shared.Clients;

public interface IAzureConfiguration
{
    string OrganizationUrl { get; }
    string RunAsManagedIdentity { get; }
    string TenantId { get; }
    string ClientId { get; }
    string ClientSecret { get; }
    string MediaStorageConnectionString { get; }
    string ResourcePrefix { get; }
    string ProjectStorageKeySecretName { get; }
    string SubscriptionId { get; }
    string EnvironmentName { get; }
    IEnumerable<string> AllowedUserEmailDomains { get; }
    string? GraphInviteFunctionUrl { get; }
    string? AddUserToGroupFunctionUrl { get; }

    const string DefaultResourcePrefix = "fsdh";
    const string DefaultProjectStorageKeySecretName = "storage-key";
}

public class AzureDevOpsClient(IAzureConfiguration config, AzAccessTokenManager tokenManager)
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
        try
        {
            var aadCredentials = await Credentials();
            var vssConnection = new VssConnection(new Uri(config.OrganizationUrl), aadCredentials);
            await vssConnection.ConnectAsync();
            return vssConnection;
        }
        catch (VssUnauthorizedException ex)
        {
            throw new InvalidOperationException($"Failed to authenticate to Azure DevOps organization '{config.OrganizationUrl}'. Ensure the managed identity or service principal has access to the organization and token acquisition is valid.", ex);
        }
    }

    private async Task<VssCredentials> Credentials()
    {
        var accessToken = await tokenManager.AccessDevopsTokenAsync();
        var aadToken = new VssAadToken("Bearer", accessToken.Token);
        return new VssAadCredential(aadToken);
    }
}
