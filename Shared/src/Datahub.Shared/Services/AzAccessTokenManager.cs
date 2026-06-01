using Azure.Core;
using Datahub.Application.Services.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Datahub.Shared.Clients;

public class AzAccessTokenManager(ISystemTokenCredentialService tokenCredentialService, [FromKeyedServices(SystemTokenCredentialServiceKeys.Infra)] ISystemTokenCredentialService infraTokenCredentialService)
{
    public const string AzureManagementApiScopeDefault = "https://management.azure.com/.default";
    public const string AzureDevopsScope = "499b84ac-1321-427f-aa17-267ca6975798";
    public const string DatabricksScope = "2ff814a6-3304-4ab8-85cb-cd0e6f879c1d/.default";

    private static string AzureDevOpsScopeDefault => $"{AzureDevopsScope}/.default";

    public async Task<AccessToken> AccessAzureManagementTokenAsync()
    {
        var tokenCredential = infraTokenCredentialService.GetTokenCredential();
        var accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext([AzureManagementApiScopeDefault]), CancellationToken.None);
        return accessToken;
    }

    public async Task<AccessToken> AccessDevopsTokenAsync()
    {
        var tokenCredential = infraTokenCredentialService.GetTokenCredential();
        return await tokenCredential.GetTokenAsync(new TokenRequestContext([AzureDevOpsScopeDefault]), CancellationToken.None);
    }

    public async Task<AccessToken> AccessDatabricksTokenAsync()
    {
        var tokenCredential = tokenCredentialService.GetTokenCredential();
        return await tokenCredential.GetTokenAsync(new TokenRequestContext([DatabricksScope]), CancellationToken.None);
    }
}
