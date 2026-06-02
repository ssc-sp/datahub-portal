using Azure.Security.KeyVault.Secrets;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.Infrastructure.Services;

public class ProjectStorageConfigurationService : IProjectStorageConfigurationService
{
    private readonly DatahubPortalConfiguration _portalConfiguration;
    private readonly IKeyVaultUserService _keyVaultService;

    public ProjectStorageConfigurationService(DatahubPortalConfiguration portalConfiguration, IKeyVaultUserService keyVaultService)
    {
        _portalConfiguration = portalConfiguration;
        _keyVaultService = keyVaultService;
    }

    public string GetProjectStorageAccountName(string projectAcronym)
    {
        var envName = GetEnvironmentName();
        return $"{_portalConfiguration.ResourcePrefix}proj{projectAcronym.ToLower()}{envName}";
    }

    public async Task<string?> GetProjectStorageAccountKey(string projectAcronym)
    {
        var secret = await _keyVaultService.GetSecretAsync(projectAcronym, GetProjectStorageKeyName(projectAcronym));
        if (secret == null) return null;
        return secret.ToString() ?? throw new InvalidOperationException("Project storage account key not found.");
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

}
