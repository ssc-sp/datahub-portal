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
using Datahub.Shared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Datahub.Shared.Services
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
        private string getEnvSuffix()
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            envName = (envName != null ? envName.ToLower() : "dev");
            if (envName.Equals("development"))
            {
                envName = "dev";
            }

            return envName;
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
                Console.WriteLine($"Could not retrieve secret: {secretName}");
                _logger.LogError($"Could not retrieve secret: {secretName}");
                _logger.LogError($"The following error occured: {e.Message}");
            }

            return string.Empty;
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

    }
}
