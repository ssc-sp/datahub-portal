using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecret(string secretName);
        Task<string> SignAsync(string data);
    }
}