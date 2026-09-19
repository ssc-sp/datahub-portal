using Datahub.Application.Services.Security;
using Datahub.Core.Model.CloudStorage;

namespace Datahub.Functions.Services
{
    public class ServerKeyVaultService : IKeyVaultUserService
    {
        public Task AuthenticateWithUserContext()
        {
            return Task.CompletedTask;
        }

        public Task DeleteAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<Uri?> GetKeyAsync(string acronym, string name)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetSecretAsync(string acronym, string name)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetSecretFromCentralKeyVaultAsync(string keyVaultName, string secretName)
        {
            throw new NotImplementedException();
        }

        public string GetVaultName(string acronym, string environment)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsKeyEnabled(string workspaceAcronym, string keyName)
        {
            throw new NotImplementedException();
        }

        public Task<bool?> IsSecretExpired(string acronym, string name)
        {
            throw new NotImplementedException();
        }

        public Task StoreAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym, IDictionary<string, string> connectionData)
        {
            throw new NotImplementedException();
        }

        public Task StoreOrUpdateSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            throw new NotImplementedException();
        }

        public Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            throw new NotImplementedException();
        }
    }
}
