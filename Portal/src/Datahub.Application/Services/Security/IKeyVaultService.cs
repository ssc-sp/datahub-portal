using Microsoft.Azure.KeyVault.Models;

namespace Datahub.Application.Services.Security;

public interface IKeyVaultService
{
    Task<KeyBundle> GetKey(string keyName);
    Task<string> GetSecret(string secretName);
    Task<string> GetClientSecret();
    Task<string> EncryptApiTokenAsync(string tokenData);
    Task<string> DecryptApiTokenAsync(string tokenData);
}