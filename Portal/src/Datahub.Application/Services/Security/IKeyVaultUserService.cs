﻿
using Datahub.Core.Model.CloudStorage;

namespace Datahub.Application.Services.Security
{
    public interface IKeyVaultUserService
    {
        Task Authenticate();
        Task<IDictionary<string,string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym);
        Task StoreAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym, IDictionary<string, string> connectionData);
        Task DeleteAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym);
        Task<string?> GetSecret(string acronym, string name);
        Task<bool?> IsSecretExpired(string acronym, string name);
        Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12);
    }
}