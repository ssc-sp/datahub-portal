using Amazon.Runtime.Internal.Util;
using Azure;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Services;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models.Search;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System.Text.RegularExpressions;

namespace Datahub.Infrastructure.Services.Security
{
	public class KeyVaultUserService : IKeyVaultUserService
    {
        private readonly IUserInformationService _userInfoService;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        private readonly IUserTokenCredentialService _tokenCredentialService;
        private readonly ISystemTokenCredentialService _systemTokenCredentialService;
        private readonly ILogger<KeyVaultUserService> _logger;
        private string? _userToken;

        public KeyVaultUserService(IUserInformationService userInfoService,
            DatahubPortalConfiguration datahubPortalConfiguration,
            IUserTokenCredentialService tokenCredentialService,
            ISystemTokenCredentialService systemTokenCredentialService,
            ILogger<KeyVaultUserService> logger, MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _userInfoService = userInfoService;
            _datahubPortalConfiguration = datahubPortalConfiguration;
            _tokenCredentialService = tokenCredentialService;
            _systemTokenCredentialService = systemTokenCredentialService;
            _logger = logger;
        }


        public async Task<SecretClient> GetSecretClient(string kvName)
        {
            var vaultURL =
                    new Uri(GetKeyVaultURL(kvName));
            if (await _userInfoService.IsExternalUser())
            {
                return new SecretClient(vaultURL, _systemTokenCredentialService.GetTokenCredential());
            }

            if (_userToken is not null)
            {
                var user = await _userInfoService.GetAuthenticatedUser();
                return new SecretClient(vaultURL, await _tokenCredentialService.GetTokenCredentialForUser(_userToken));

            }
            else
            {
                return new SecretClient(vaultURL, _systemTokenCredentialService.GetTokenCredential());
            }
        }

        public async Task<KeyClient> GetKeyClient(string kvName)
        {
            var vaultURL = new Uri(GetKeyVaultURL(kvName));
            if (await _userInfoService.IsExternalUser())
            {
                return new KeyClient(vaultURL, _systemTokenCredentialService.GetTokenCredential());
            }

            if (_userToken is not null)
            {
                return new KeyClient(vaultURL, await _tokenCredentialService.GetTokenCredentialForUser(_userToken));
            }

            return new KeyClient(vaultURL, _systemTokenCredentialService.GetTokenCredential());
        }

        public async Task<SecretClient> GetWorkspaceSecretClient(string workspace) => await GetSecretClient(GetVaultName(workspace.ToLowerInvariant(),
_datahubPortalConfiguration.Hosting.EnvironmentName));

        public async Task<KeyClient> GetWorkspaceKeyClient(string workspace) => await GetKeyClient(GetVaultName(workspace.ToLowerInvariant(),
_datahubPortalConfiguration.Hosting.EnvironmentName));

        //    rg_name = f"fsdh_proj_{workspace_definition['Workspace']['Acronym']}_{environment_name}_rg"
        // vault_name = f"fsdh-proj-{workspace_definition['Workspace']['Acronym']}-{environment_name}-kv"

        public string GetVaultName(string acronym, string environmentName) =>
            $"fsdh-proj-{acronym}-{environmentName}-kv";

        public string GetKeyVaultURL(string vaultName) => $"https://{vaultName}.vault.azure.net/";

        public async Task<bool> IsKeyEnabled(string workspaceAcronym, string keyName)
        {
            var key = await GetKVKey(workspaceAcronym, keyName);
            return key?.Properties.Enabled ?? false;
        }

        public async Task<string?> GetSecretFromCentralKeyVaultAsync(string keyVaultName, string name)
        {

            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            KeyVaultSecret? secret = null;
            try
            {
                secret = await (await GetSecretClient(keyVaultName)).GetSecretAsync(secretName);
            }
            catch (RequestFailedException kvex)
            {
                _logger.LogError(kvex, $"Error retrieving secret {secretName} from key vault {keyVaultName}.");
                return null;
            }

            return secret?.Value;
        }

        public async Task<string?> GetSecretAsync(string acronym, string name) => (await GetKVSecret(acronym, name))?.Value;

        public async Task<Uri?> GetKeyAsync(string acronym, string name) => (await GetKVKey(acronym, name))?.Id;

        private async Task<KeyVaultSecret?> GetKVSecret(string acronym, string name)
        {
            var secretName = CleanName(name);
            // This retrieves the secret/certificate with the private key
            KeyVaultSecret? secret = null;
            try
            {
                secret = await (await GetWorkspaceSecretClient(acronym)).GetSecretAsync(secretName);
            }
            catch (RequestFailedException kvex)
            {
                _logger.LogError(kvex, $"Error retrieving secret {secretName} from key vault {acronym}.");
                return null;
            }

            return secret;
        }

        private async Task<KeyVaultKey?> GetKVKey(string acronym, string name)
        {
            var keyName = CleanName(name);
            KeyVaultKey? key = null;
            try
            {
                key = await (await GetWorkspaceKeyClient(acronym)).GetKeyAsync(keyName);
            }
            catch (RequestFailedException kvex)
            {
                _logger.LogError(kvex, $"Error retrieving key {keyName} from key vault {acronym}.");
                return null;
            }

            return key;
        }

        public async Task<bool?> IsSecretExpired(string acronym, string name)
        {
            var secret = await GetKVSecret(acronym, name);
            if (secret == null)
                return true; // If the secret doesn't exist, we can consider it as expired or invalid
            if (secret.Properties.ExpiresOn.HasValue)
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
            var secretName = CleanName(name);
            var client = await GetWorkspaceSecretClient(acronym);

            var secret = new KeyVaultSecret(secretName, secretValue)
            {
                Properties =
                {
                    Enabled = true,
                    ExpiresOn = DateTimeOffset.UtcNow.AddMonths(monthValidity),
                    NotBefore = DateTimeOffset.UtcNow
                }
            };

            await client.SetSecretAsync(secret);
        }

        public async Task StoreOrUpdateSecret(string acronym, string name, string secretValue, int monthValidity = 12)
        {
            var secretName = CleanName(name);
            var client = await GetWorkspaceSecretClient(acronym);

            var secret = new KeyVaultSecret(secretName, secretValue)
            {
                Properties =
                {
                    Enabled = true,
                    ExpiresOn = DateTimeOffset.UtcNow.AddMonths(monthValidity),
                    NotBefore = DateTimeOffset.UtcNow
                }
            };

            await client.SetSecretAsync(secret);
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
            var client = await GetWorkspaceSecretClient(acronym);
            var cleanedSecretName = CleanName(secretName);

            try
            {
                var operation = await client.StartDeleteSecretAsync(cleanedSecretName);
                return operation != null;
            }
            catch (RequestFailedException ex) when (ex.Status == 404 || ex.ErrorCode == "SecretNotFound")
            {
                return false;
            }
        }

        public async Task AuthenticateWithUserContext()
        {
            try
            {
                _userToken = await _tokenCredentialService.GetUserToken(await _userInfoService.GetAuthenticatedUser(), IUserTokenCredentialService.KEYVAULT_SERVICE);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogWarning(ex, "Failed to authenticate Key Vault with user context due to user challenge/consent issue. Falling back to app context.");
                throw;
            }
            catch (MsalUiRequiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while authenticating Key Vault with user context.");
                throw;
            }
        }
    }
}
