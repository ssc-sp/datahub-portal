using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecret(string secretName);
        Task<string> EncryptApiTokenAsync(string tokenData);
        Task<string> DecryptApiTokenAsync(string tokenData);
    }
}