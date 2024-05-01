using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Services;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Text.RegularExpressions;
using Datahub.Application.Services.UserManagement;

namespace Datahub.Infrastructure.Services.Security
{
	public class KeyVaultUserService : IDisposable, IKeyVaultUserService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IUserInformationService _userInfoService;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        private readonly ILogger<KeyVaultUserService> _logger;
        private string _vaultToken;
        private KeyVaultClient? _keyVaultClient = null;

        public KeyVaultUserService(ITokenAcquisition tokenAcquisition,
            IUserInformationService userInfoService,
            DatahubPortalConfiguration datahubPortalConfiguration,
            ILogger<KeyVaultUserService> logger, MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _tokenAcquisition = tokenAcquisition;
            _userInfoService = userInfoService;
            _datahubPortalConfiguration = datahubPortalConfiguration;
            _logger = logger;
        }

        public async Task Authenticate()
        {
            var user = await _userInfoService.GetAuthenticatedUser();
            var scopes = new string[] { "https://vault.azure.net/user_impersonation" };
            _vaultToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes, user: user);
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetUserAccessToken));
        }

        private async Task<string> GetUserAccessToken(string auth, string res, string scope)
        {
            return await Task.FromResult(_vaultToken);
        }

        //    rg_name = f"fsdh_proj_{workspace_definition['Workspace']['Acronym']}_{environment_name}_rg"
        // vault_name = f"fsdh-proj-{workspace_definition['Workspace']['Acronym']}-{environment_name}-kv"

        public string GetVaultName(string acronym, string environmentName) =>
            $"fsdh-proj-{acronym}-{environmentName}-kv";

        public string GetKeyVaultURL(string vaultName) => $"https://{vaultName}.vault.azure.net/";

        public async Task<string?> GetSecretAsync(string acronym, string name)
        {
            if (_keyVaultClient is null)
            {
                try
                {
                    await Authenticate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error authenticating KeyVaultUserService");
                    throw new InvalidOperationException("KeyVaultUserService not authenticated");
                }
            }

            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            SecretBundle secret = null;
            var vaultName =
                GetKeyVaultURL(GetVaultName(acronym.ToLowerInvariant(),
                    _datahubPortalConfiguration.Hosting.EnvironmentName));
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

                _logger.LogError(kvex, "Error retrieving secret {0} from vault {1}", secretName, vaultName);
                throw;
            }

            return secret.Value;
        }

        public async Task<bool?> IsSecretExpired(string acronym, string name)
        {
            if (_keyVaultClient is null) throw new InvalidOperationException("KeyVaultUserService not authenticated");

            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            SecretBundle secret = null;
            try
            {
                secret = await this._keyVaultClient.GetSecretAsync(
                    GetKeyVaultURL(
                        GetVaultName(acronym.ToLowerInvariant(), _datahubPortalConfiguration.Hosting.EnvironmentName)),
                    secretName);
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

        private static string CleanName(string name)
        {
            Regex regex = new Regex("[^a-zA-Z0-9-]");
            return regex.Replace(name, "");
        }

        public async Task StoreSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            if (_keyVaultClient is null) throw new InvalidOperationException("KeyVaultUserService not authenticated");
            var secretName = CleanName(name);
            var secretAttributes = new SecretAttributes()
            {
                Enabled = true,
                Expires = DateTimeOffset.UtcNow.AddMonths(monthValidity).DateTime,
                NotBefore = DateTimeOffset.UtcNow.DateTime
            };
            await _keyVaultClient.SetSecretAsync(
                GetKeyVaultURL(GetVaultName(acronym.ToLowerInvariant(),
                    _datahubPortalConfiguration.Hosting.EnvironmentName)),
                secretName, secretValue, secretAttributes: secretAttributes);
        }

        public async Task StoreOrUpdateSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            try
            {
                await StoreSecret(acronym, name, secretValue, monthValidity);
            }
            catch (KeyVaultErrorException kvex)
            {
                if (kvex.Body.Error.Code == "SecretAlreadyExists")
                {
                    var secretAttributes = new SecretAttributes()
                    {
                        Enabled = true,
                        Expires = DateTimeOffset.UtcNow.AddMonths(monthValidity).DateTime,
                        NotBefore = DateTimeOffset.UtcNow.DateTime
                    };
                    await _keyVaultClient.UpdateSecretAsync(
                        GetKeyVaultURL(GetVaultName(acronym.ToLowerInvariant(),
                            _datahubPortalConfiguration.Hosting.EnvironmentName)),
                        name, string.Empty, secretAttributes: secretAttributes);
                }
                else
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            if (_keyVaultClient != null)
                ((IDisposable)_keyVaultClient).Dispose();
        }

        public static string GetSecretNameForStorage(int id, string name) => CleanName($"st-{id}-{name}");

        public async Task<IDictionary<string, string>> GetAllSecrets(ProjectCloudStorage projectCloudStorage,
            string acronym)
        {
            var secrets = new Dictionary<string, string>();
            foreach (var secretKey in CloudStorageHelpers.All_Keys)
            {
                var secretValue =
                    await GetSecretAsync(acronym, GetSecretNameForStorage(projectCloudStorage.Id, secretKey));
                if (secretValue != null)
                    secrets.Add(secretKey, secretValue);
            }

            return secrets;
        }

        public async Task StoreAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym,
            IDictionary<string, string> connectionData)
        {
            foreach (var secretKey in CloudStorageHelpers.All_Keys)
            {
                if (connectionData.ContainsKey(secretKey) && !string.IsNullOrEmpty(connectionData[secretKey]))
                {
                    await StoreSecret(acronym, GetSecretNameForStorage(projectCloudStorage.Id, secretKey),
                        connectionData[secretKey]);
                }
            }
        }

        public async Task DeleteAllSecrets(ProjectCloudStorage projectCloudStorage, string acronym)
        {
            foreach (var secretKey in CloudStorageHelpers.All_Keys)
            {
                await TryDeleteSecret(acronym, GetSecretNameForStorage(projectCloudStorage.Id, secretKey));
            }
        }

        private async Task<bool> TryDeleteSecret(string acronym, string secretName)
        {
            try
            {
                var secret = await _keyVaultClient.DeleteSecretAsync(
                    GetKeyVaultURL(
                        GetVaultName(acronym.ToLowerInvariant(), _datahubPortalConfiguration.Hosting.EnvironmentName)),
                    secretName);
                if (secret != null)
                    return true;
                return false;
            }
            catch (KeyVaultErrorException kvex)
            {
                if (kvex.Body.Error.Code == "SecretNotFound")
                {
                    return false;
                }

                throw;
            }
        }
    }
}