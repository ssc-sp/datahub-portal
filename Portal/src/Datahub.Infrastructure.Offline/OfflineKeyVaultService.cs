using Datahub.Application.Services.Security;

namespace Datahub.Infrastructure.Offline;

public class OfflineKeyVaultService : IKeyVaultService
{
    public Task<string> GetSecret(string secretName) => Task.FromResult(string.Empty);
    public Task<string> EncryptApiTokenAsync(string data) => Task.FromResult(data);
    public Task<string> DecryptApiTokenAsync(string data) => Task.FromResult(data);
    public Task<string> GetClientSecret() => Task.FromResult(string.Empty);
}