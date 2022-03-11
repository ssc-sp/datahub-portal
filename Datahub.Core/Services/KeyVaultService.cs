using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Datahub.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.WebKey;
using System.Text;
using System.Security.Cryptography;

namespace Datahub.Core.Services
{
    public class KeyVaultService : IKeyVaultService
    {
        private IConfiguration _configuration;
        private IWebHostEnvironment _webHostEnvironment;
        private ILogger<KeyVaultService> _logger;
        public KeyVaultClient _keyVaultClient;
        private IOptions<APITarget> _targets;

        public KeyVaultService(IWebHostEnvironment webHostEnvironment, IConfiguration configureOptions, IOptions<APITarget> targets, ILogger<KeyVaultService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configureOptions;
            _logger = logger;
            _targets = targets;
        }

        public async Task<string> GetSecret(string secretName)
        {
            try
            {
                if (_keyVaultClient == null)
                {
                    SetKeyVaultClient();
                }

                var keyVaultName = _targets.Value.KeyVaultName;
                var keyValueSecret = await _keyVaultClient.GetSecretAsync("https://" + keyVaultName + ".vault.azure.net/", secretName);
                return keyValueSecret.Value;
            }
            catch (Exception e)
            {
                _logger.LogError($"Could not retrieve secret: {secretName}");
                _logger.LogError($"The following error occured: {e.Message}");
                _logger.LogError(e,$"Could not retrieve secret: {secretName}");
                throw;
            }
        }

        public async Task<string> EncryptApiTokenAsync(string data)
        {
            if (_keyVaultClient == null)
            {
                SetKeyVaultClient();
            }

            string keyIdentifier = GetApiKeyIdentifier();
            var encrypedData = await _keyVaultClient.EncryptAsync(keyIdentifier, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Encoding.UTF8.GetBytes(data));

            return Convert.ToBase64String(encrypedData.Result);
        }

        public async Task<string> DecryptApiTokenAsync(string data)
        {
            if (_keyVaultClient == null)
            {
                SetKeyVaultClient();
            }

            string keyIdentifier = GetApiKeyIdentifier();
            var decrypedData = await _keyVaultClient.DecryptAsync(keyIdentifier, JsonWebKeyEncryptionAlgorithm.RSAOAEP, System.Convert.FromBase64String(data));

            return Encoding.UTF8.GetString(decrypedData.Result);
        }

        private string GetApiKeyIdentifier()
        {
            var keyVaultName = _targets.Value.KeyVaultName;
            var keyPath = _targets.Value.KeyVaultApiKeyPath;
            return $"https://{keyVaultName}.vault.azure.net/keys/{keyPath}";
        }

        private void SetKeyVaultClient()
        {
            DefaultAzureCredential credential;
            _logger.LogInformation("Entering setting Key Vault");
            if (_webHostEnvironment.IsDevelopment() || Environment.GetEnvironmentVariable("IS_LOCAL") != null)
            {
                //for these credentials, check config file for user name
                //check your ide account ca connect to azure - Tools -> Options -> Azure Service Authentication
                //check keyvault access for user permissions
                var azureCredentialOptions = new DefaultAzureCredentialOptions();
                azureCredentialOptions.SharedTokenCacheUsername = _configuration["KeyVault:UserName"];
                credential = new DefaultAzureCredential(azureCredentialOptions);
                _keyVaultClient = new KeyVaultClient((authority, resource, scope) =>
                {
                    var token = credential.GetToken(
                        new Azure.Core.TokenRequestContext(
                            new[] { "https://vault.azure.net/.default" }));
                    return Task.FromResult(token.Token);
                });
            }
            else
            {
                _logger.LogInformation("Entering key vault production");
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            }

        }

        static byte[] GetSHA256Digest(string value)
        {
            using var hash = SHA256.Create();
            return hash.ComputeHash(Encoding.UTF8.GetBytes(value));
        }
    }
}
