using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace NRCan.Datahub.Shared.Services
{
    public interface IProjectDatabaseService
    {
        Task<string> GetPostgresToken();
        Task<AppAuthenticationResult> GetPostgresAuthenticationObject();
        bool IsServiceAvailable();
    }
}
