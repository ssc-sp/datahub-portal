using System.Threading.Tasks;

namespace Datahub.Shared.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecret(string secretName);
    }
}