using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Services;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Security
{
    public class KeyVaultUserService : IDisposable, IKeyVaultUserService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IUserInformationService _userInfoService;
        private readonly DatahubPortalConfiguration portalConfiguration;
        private readonly ILogger<CloudStorageManagerFactory> logger;
        private readonly KeyVaultClient _keyVaultClient;

        public KeyVaultUserService(ITokenAcquisition tokenAcquisition, 
            IUserInformationService userInfoService, 
            DatahubPortalConfiguration portalConfiguration,
            ILogger<CloudStorageManagerFactory> logger)
        {
            _tokenAcquisition = tokenAcquisition;
            _userInfoService = userInfoService;
            this.portalConfiguration = portalConfiguration;
            this.logger = logger;
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetUserAccessToken));
        }

        private async Task<string> GetUserAccessToken(string auth, string res, string scope)
        {
            var scopes = new string[] { "https://vault.azure.net/user_impersonation" };
            var user = await _userInfoService.GetAuthenticatedUser();
            var result = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes, user: user);

            return await Task.FromResult(result);
        }

        //    rg_name = f"fsdh_proj_{workspace_definition['Workspace']['Acronym']}_{environment_name}_rg"
        // vault_name = f"fsdh-proj-{workspace_definition['Workspace']['Acronym']}-{environment_name}-kv"

        public string GetVaultName(string acronym, string environmentName) => $"fsdh-proj-{acronym}-{environmentName}-kv";

        public string GetKeyVaultURL(string vaultName) => $"https://{vaultName}.vault.azure.net/";

        public async Task<string?> GetSecret(string acronym, string name)
        {
            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            SecretBundle secret = null;
            var vaultName = GetKeyVaultURL(GetVaultName(acronym.ToLowerInvariant(), portalConfiguration.Hosting.EnvironmentName));
            try
            {
                secret = await this._keyVaultClient.GetSecretAsync(vaultName, secretName);
            }
            catch (KeyVaultErrorException kvex)
            {
                if (kvex.Body.Error.Code == "SecretNotFound")
                {
                    return null;
                }
                logger.LogError(kvex, "Error retrieving secret {0} from vault {1}", secretName, vaultName);
                throw;
            }
            return secret.Value;
        }

        public async Task<bool?> IsSecretExpired(string acronym, string name)
        {
            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            SecretBundle secret = null;
            try
            {
                secret = await this._keyVaultClient.GetSecretAsync(GetKeyVaultURL(GetVaultName(acronym.ToLowerInvariant(), portalConfiguration.Hosting.EnvironmentName)), secretName);
            }
            catch (KeyVaultErrorException kvex)
            {
                if (kvex.Body.Error.Code == "SecretNotFound")
                {
                    return null;
                }
                throw;
            }
            // Check if the secret is expired
            if (secret.Attributes.Expires.HasValue)
            {
                return DateTimeOffset.UtcNow > secret.Attributes.Expires.Value;
            }
            else
            {
                // If the Expires property is not set, we assume the secret is not expired
                return false;
            }
        }

        private string CleanName(string name)
        {
            Regex regex = new Regex("[^a-zA-Z0-9-]");
            return regex.Replace(name, "");
        }

        public async Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            var secretName = CleanName(name);
            var secretAttributes = new SecretAttributes()
            {
                Enabled = true,
                Expires = DateTimeOffset.UtcNow.AddMonths(monthValidity).DateTime,
                NotBefore = DateTimeOffset.UtcNow.DateTime
            };
            await _keyVaultClient.SetSecretAsync(GetKeyVaultURL(GetVaultName(acronym.ToLowerInvariant(), portalConfiguration.Hosting.EnvironmentName)), secretName, secretValue, secretAttributes: secretAttributes);
        }

        public void Dispose()
        {
            ((IDisposable)_keyVaultClient).Dispose();
        }

        public string GetSecretNameForStorage(ProjectCloudStorage projectCloudStorage, string name) => CleanName($"st-{projectCloudStorage.Id}-{name}");

        public async Task<IDictionary<string, string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            var secrets = new Dictionary<string, string>();
            foreach (var secretKey in CloudStorageHelpers.All_Keys)
            {
                var secretValue = await GetSecret(acronym, GetSecretNameForStorage(projectCloudStorage, secretKey));
                if (secretValue != null)
                    secrets.Add(secretKey, secretValue);
            }
            return secrets;
        }

        public async Task StoreAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym, IDictionary<string, string> connectionData)
        {
            foreach (var secretKey in CloudStorageHelpers.All_Keys)
            {
                if (connectionData.ContainsKey(secretKey) && !string.IsNullOrEmpty(connectionData[secretKey]))
                {
                    await StoreSecret(acronym, GetSecretNameForStorage(projectCloudStorage, secretKey), connectionData[secretKey]);
                }
            }
        }
    }
}
