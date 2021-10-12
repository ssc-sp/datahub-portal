using System.Threading.Tasks;

namespace Datahub.Portal.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecret(string secretName);
    }
}