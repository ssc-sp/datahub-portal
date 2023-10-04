using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.Infrastructure.Services;

public class ProjectStorageConfigurationService : IProjectStorageConfigurationService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;

    public ProjectStorageConfigurationService(DatahubPortalConfiguration portalConfiguration)
    {
        _portalConfiguration = portalConfiguration;
    }

    public string GetProjectStorageAccountName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}proj{projectAcronym.ToLower()}{envName}";
    }

    public async Task<string> GetProjectStorageAccountKey(string projectAcronym)
    {
        var accountKey = await GetProjectStorageAccountKeyAsync(projectAcronym);
        return accountKey.Value;
    }

    private async Task<SecretBundle> GetProjectStorageAccountKeyAsync(string projectAcronym)
    {
        var key = GetProjectStorageKeyName(projectAcronym);
        var keyVaultName = GetProjectKeyVaultName(projectAcronym);
        var keyVaultClient = GetKeyVaultClient();
        var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net";
        return await keyVaultClient.GetSecretAsync(keyVaultUrl, key);
    }

    private static string GetEnvironmentName()
    {
        var envName = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev").ToLower();

        // map developemnt or sandbox to dev
        if (envName.Equals("development") || envName.Equals("sand"))
            return "dev";

        return envName;
    }

    private string GetProjectStorageKeyName(string projectAcronym)
    {
        if (_portalConfiguration.CentralizedProjectSecrets)
        {
            return $"datahub-blob-key-{projectAcronym.ToLower()}";
        }

        return _portalConfiguration.ProjectStorageKeySecretName;
    }

    private string GetProjectKeyVaultName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}-proj-{projectAcronym}-{envName}-kv".ToLower();
    }

    private KeyVaultClient GetKeyVaultClient()
    {
        AzureServiceTokenProvider azureServiceTokenProvider;

        if (_portalConfiguration.PortalRunAsManagedIdentity.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
        {
            azureServiceTokenProvider = new AzureServiceTokenProvider("RunAs=App");
        }
        else
        {
            var tenantId = _portalConfiguration.AzureAd.TenantId;
            var clientId = _portalConfiguration.AzureAd.ClientId;
            var clientSecret = _portalConfiguration.AzureAd.ClientSecret;

            azureServiceTokenProvider =
                new AzureServiceTokenProvider($"RunAs=App;AppId={clientId};TenantId={tenantId};AppKey={clientSecret}");
        }

        return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
    }
}
