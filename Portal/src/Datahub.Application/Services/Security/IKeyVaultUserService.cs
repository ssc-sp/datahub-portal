using Datahub.Core.Model.CloudStorage;

namespace Datahub.Application.Services.Security
{
    public interface IKeyVaultUserService
    {
        Task AuthenticateWithUserContext();
        Task<IDictionary<string, string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym);

        Task StoreAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym,
            IDictionary<string, string> connectionData);

        Task DeleteAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym);
        Task<string?> GetSecretAsync(string acronym, string name);
        Task<string?> GetSecretFromCentralKeyVaultAsync(string keyVaultName, string secretName);
        Task<bool?> IsSecretExpired(string acronym, string name);
        string GetVaultName(string acronym, string environment);
        Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12);
        Task StoreOrUpdateSecret(string acronym, string name, string secretValue, int monthValidity = 12);
    }
}