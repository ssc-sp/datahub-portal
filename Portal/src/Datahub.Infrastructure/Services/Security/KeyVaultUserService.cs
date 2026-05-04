using Amazon.Runtime.Internal.Util;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Services;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.Search;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Text.RegularExpressions;

namespace Datahub.Infrastructure.Services.Security
{
	public class KeyVaultUserService : IDisposable, IKeyVaultUserService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IUserInformationService _userInfoService;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        private readonly ITokenCredentialService _tokenCredentialService;
        private readonly ILogger<KeyVaultUserService> _logger;
        private string? _vaultToken;

        public KeyVaultUserService(ITokenAcquisition tokenAcquisition,
            IUserInformationService userInfoService,
            DatahubPortalConfiguration datahubPortalConfiguration,
            ITokenCredentialService tokenCredentialService,
            ILogger<KeyVaultUserService> logger, MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _tokenAcquisition = tokenAcquisition;
            _userInfoService = userInfoService;
            _datahubPortalConfiguration = datahubPortalConfiguration;
            _tokenCredentialService = tokenCredentialService;
            _logger = logger;
        }


        public async Task<SecretClient> GetSecretClient(string workspace)
        {
            var vaultURL =
                    new Uri(GetKeyVaultURL(GetVaultName(workspace.ToLowerInvariant(),
                    _datahubPortalConfiguration.Hosting.EnvironmentName)));
            if (await _userInfoService.IsExternalUser())
            {
                return new SecretClient(vaultURL, _tokenCredentialService.GetTokenCredential());
            }

            try
            {
                var user = await _userInfoService.GetAuthenticatedUser();
                return new SecretClient(vaultURL, await _tokenCredentialService.GetTokenCredentialForUser(user));

            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogWarning(ex, "Failed to authenticate Key Vault with user context due to user challenge/consent issue. Falling back to app context.");
                return new SecretClient(vaultURL, _tokenCredentialService.GetTokenCredential());
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogWarning(ex, "Failed to authenticate Key Vault with user context due to identity error. Falling back to app context.");
                return new SecretClient(vaultURL, _tokenCredentialService.GetTokenCredential());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while authenticating Key Vault with user context.");
                throw;
            }
        }

        private async Task<string> GetUserAccessToken(string auth, string res, string scope)
        {
            return await Task.FromResult(_vaultToken ?? string.Empty);
        }



        //    rg_name = f"fsdh_proj_{workspace_definition['Workspace']['Acronym']}_{environment_name}_rg"
        // vault_name = f"fsdh-proj-{workspace_definition['Workspace']['Acronym']}-{environment_name}-kv"

        public string GetVaultName(string acronym, string environmentName) =>
            $"fsdh-proj-{acronym}-{environmentName}-kv";

        public string GetKeyVaultURL(string vaultName) => $"https://{vaultName}.vault.azure.net/";

        public async Task<string?> GetSecretFromCentralKeyVaultAsync(string keyVaultName, string secretName)
        {
            if (_keyVaultClient is null)
            {
                try
                {
                    await AuthenticateWithUserContext();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error authenticating KeyVaultUserService");
                    throw new InvalidOperationException("KeyVaultUserService not authenticated");
                }
            }

            var cleanedSecretName = CleanName(secretName);
            var vaultUrl = GetKeyVaultURL(keyVaultName);
            
            try
            {
                var secret = await _keyVaultClient.GetSecretAsync(vaultUrl, cleanedSecretName);
                return secret?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving secret from central key vault.");
                return null;
            }
        }

        public async Task<string?> GetSecretAsync(string acronym, string name) => (await GetKVSecret(acronym, name))?.Value;

        private async Task<KeyVaultSecret?> GetKVSecret(string acronym, string name)
        {
            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            KeyVaultSecret? secret = null;
            try
            {
                secret = await (await GetSecretClient(acronym)).GetSecretAsync(secretName);
            }
            catch (RequestFailedException kvex)
            {
                _logger.LogError(kvex, "Error retrieving secret from key vault.");
                return null;
            }

            return secret;
        }

        public async Task<bool?> IsSecretExpired(string acronym, string name)
        {
            var secret = await GetKVSecret(acronym, name);
            if (secret == null)
                return true; // If the secret doesn't exist, we can consider it as expired or invalid
            if (secret?.Properties?.ExpiresOn.HasValue == true)
            {
                return DateTimeOffset.UtcNow > secret.Properties.ExpiresOn.Value;
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
