using Datahub.Application.Services.Security;
using Datahub.Core.Model.CloudStorage;

namespace Datahub.Infrastructure.Offline.Security
{
	public class OfflineKeyVaultUserService : IKeyVaultUserService
    {
        public async Task Authenticate()
        {
            // nothing to do
        }

        public Task DeleteAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetAllSecrets(string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetSecretAsync(string acronym, string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool?> IsSecretExpired(string acronym, string name)
        {
            throw new NotImplementedException();
        }

        public string GetVaultName(string acronym, string environment)
        {
            throw new NotImplementedException();
        }

        public Task StoreAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym, IDictionary<string, string> connectionData)
        {
            throw new NotImplementedException();
        }

        public Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            throw new NotImplementedException();
        }

        public Task StoreOrUpdateSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            throw new NotImplementedException();
        }
    }
}
