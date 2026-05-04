using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Secrets;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using RequestFailedException = Azure.RequestFailedException;

namespace Datahub.Infrastructure.Services.Security;

public class KeyVaultCoreService : IKeyVaultService
{
    private readonly ILogger<KeyVaultCoreService> _logger;
    private readonly IOptions<APITargets> _targets;
    private readonly ISystemTokenCredentialService _tokenCredentialService;

    public KeyVaultCoreService(
        IOptions<APITargets> targets,
        ILogger<KeyVaultCoreService> logger,
        ISystemTokenCredentialService tokenCredentialService)
    {
        _logger = logger;
        _targets = targets;
        _tokenCredentialService = tokenCredentialService;
    }

    public async Task<bool> IsKeyEnabled(string keyName)
    {
        try
        {
            string keyVaultName = GetKeyVaultName();
            KeyVaultSecret keyValueKey = await GetSecretClient().GetSecretAsync("https://" + keyVaultName + ".vault.azure.net", keyName);
            if (keyValueKey is null) { return false; }
            return keyValueKey.Properties.Enabled ?? false;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error checking availability of key: {keyName}");
            throw;
        }
    }

    private string GetKeyVaultName()
    {
        var keyVaultName = _targets.Value.KeyVaultName;
        if (string.IsNullOrEmpty(keyVaultName))
            throw new ArgumentNullException($"{nameof(APITargets)}__{nameof(APITargets.KeyVaultName)}", "KeyVaultName is not configured");
        return keyVaultName;
    }

    public async Task<string> GetSecret(string secretName)
    {
        try
        {
            string keyVaultName = GetKeyVaultName();

            var keyValueSecret = await GetSecretClient().GetSecretAsync("https://" + keyVaultName + ".vault.azure.net/", secretName);
            return keyValueSecret.Value.Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Could not retrieve secret: {secretName}");
            throw;
        }
    }

    public Task<string> GetClientSecret() => GetSecret("datahubportal-client-secret");

    public async Task<string> EncryptApiTokenAsync(string data)
    {

        string keyIdentifier = GetApiKeyIdentifier();
        var key = await GetKeyClient().GetKeyAsync(keyIdentifier);
        var cryptoClient = new CryptographyClient(key.Value.Id, _tokenCredentialService.GetPortalTokenCredential());

        var encryptResult = await cryptoClient.EncryptAsync(
            EncryptionAlgorithm.RsaOaep256,
            Encoding.UTF8.GetBytes(data));

        byte[] ciphertext = encryptResult.Ciphertext;

        return Convert.ToBase64String(ciphertext);
    }

    public async Task<string> DecryptApiTokenAsync(string data)
    {
        string keyIdentifier = GetApiKeyIdentifier();
        var key = await GetKeyClient().GetKeyAsync(keyIdentifier);
        var cryptoClient = new CryptographyClient(key.Value.Id, _tokenCredentialService.GetPortalTokenCredential());

        var decryptResult = await cryptoClient.DecryptAsync(
            EncryptionAlgorithm.RsaOaep256,
            Convert.FromBase64String(data));

        return Encoding.UTF8.GetString(decryptResult.Plaintext);
    }

    private string GetApiKeyIdentifier()
    {
        var keyVaultName = _targets.Value.KeyVaultName;
        var keyPath = _targets.Value.KeyVaultApiKeyPath;
        return $"https://{keyVaultName}.vault.azure.net/keys/{keyPath}";
    }

    private SecretClient GetSecretClient() => new SecretClient(new Uri(GetKeyVaultName()), _tokenCredentialService.GetPortalTokenCredential());
    private KeyClient GetKeyClient() => new KeyClient(new Uri(GetKeyVaultName()), _tokenCredentialService.GetPortalTokenCredential());
}
