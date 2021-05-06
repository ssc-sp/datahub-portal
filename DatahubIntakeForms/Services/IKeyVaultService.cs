using System.Threading.Tasks;

namespace NRCan.Datahub.Portal.Services
{
    public interface IKeyVaultService
    {
        Task<string> GetSecret(string secretName);
    }
}