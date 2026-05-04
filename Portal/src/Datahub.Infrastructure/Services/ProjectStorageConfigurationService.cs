using Azure.Security.KeyVault.Secrets;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.Infrastructure.Services;

public class ProjectStorageConfigurationService : IProjectStorageConfigurationService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;
    private readonly ISystemTokenCredentialService _tokenCredentialService;

    public ProjectStorageConfigurationService(DatahubPortalConfiguration portalConfiguration, ISystemTokenCredentialService tokenCredentialService)
    {
        _portalConfiguration = portalConfiguration;
        _tokenCredentialService = tokenCredentialService;
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

    private async Task<KeyVaultSecret> GetProjectStorageAccountKeyAsync(string projectAcronym)
    {
        var key = GetProjectStorageKeyName(projectAcronym);
        var keyVaultName = GetProjectKeyVaultName(projectAcronym);
        
        var secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), _tokenCredentialService.GetPortalTokenCredential());
        var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net";
        return await secretClient.GetSecretAsync(keyVaultUrl, key);
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

}
