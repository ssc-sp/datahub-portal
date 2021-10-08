using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.Core.Services
{
    public class OfflineProjectDatabaseService : IProjectDatabaseService
    {
        public async Task<AppAuthenticationResult> GetPostgresAuthenticationObject()
        {
            return await Task.FromResult<AppAuthenticationResult>(null);
        }

        public async Task<string> GetPostgresToken()
        {
            return await Task.FromResult<string>(null);
        }

        public bool IsServiceAvailable()
        {
            return false;
        }
    }
}