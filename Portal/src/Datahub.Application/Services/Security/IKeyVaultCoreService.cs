namespace Datahub.Application.Services.Security;

public interface IKeyVaultCoreService
{
    Task<bool> IsKeyEnabled(string keyName);
    Task<string> GetSecret(string secretName);
    Task<string> GetClientSecret();
    Task<string> EncryptApiTokenAsync(string tokenData);
    Task<string> DecryptApiTokenAsync(string tokenData);
}
