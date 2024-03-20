using System.Security.Cryptography;
using System.Text;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Datahub.Infrastructure.Services.Security;

public class KeyVaultCoreService : IKeyVaultService
{
    private DatahubPortalConfiguration _portalConfiguration;
    private ILogger<KeyVaultCoreService> _logger;
    private KeyVaultClient _keyVaultClient;
    private IOptions<APITarget> _targets;

    public KeyVaultCoreService(IOptions<APITarget> targets, ILogger<KeyVaultCoreService> logger, DatahubPortalConfiguration portalConfiguration)
    {
        _logger = logger;
        _portalConfiguration = portalConfiguration;
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
            _logger.LogError(e, $"Could not retrieve secret: {secretName}");
            throw;
        }
    }

    public Task<string> GetClientSecret() => GetSecret("datahubportal-client-secret");

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
        var decrypedData = await _keyVaultClient.DecryptAsync(keyIdentifier, JsonWebKeyEncryptionAlgorithm.RSAOAEP, Convert.FromBase64String(data));

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
        if (_portalConfiguration.PortalRunAsManagedIdentity.Equals("enabled", StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogInformation("Entering key vault production with Managed Identity");
            var azureServiceTokenProvider = new AzureServiceTokenProvider("RunAs=App");
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        }
        else
        {
            var tenantId = _portalConfiguration.AzureAd.TenantId;
            var clientId = _portalConfiguration.AzureAd.ClientId;
            var clientSecret = _portalConfiguration.AzureAd.ClientSecret;
            
            _logger.LogInformation("Entering key vault production with default identity");
            var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={clientId};TenantId={tenantId};AppKey={clientSecret}");
            _keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        }
    }

    static byte[] GetSHA256Digest(string value)
    {
        using var hash = SHA256.Create();
        return hash.ComputeHash(Encoding.UTF8.GetBytes(value));
    }
}