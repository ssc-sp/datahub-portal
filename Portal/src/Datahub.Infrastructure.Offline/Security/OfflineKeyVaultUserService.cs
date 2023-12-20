using Datahub.Application.Services.Security;
using Datahub.Core.Model.CloudStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Offline.Security
{
    public class OfflineKeyVaultUserService : IKeyVaultUserService
    {
        public async Task Authenticate()
        {
            // nothing to do
        }

        public Task<IDictionary<string, string>> GetAllSecrets(string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetSecret(string acronym, string name)
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

        public Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            throw new NotImplementedException();
        }
    }
}
