using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.Core.Services
{
    public interface IProjectDatabaseService
    {
        Task<string> GetPostgresToken();
        Task<AppAuthenticationResult> GetPostgresAuthenticationObject();
        bool IsServiceAvailable();
    }
}
