using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecret(string secretName);
    }
}