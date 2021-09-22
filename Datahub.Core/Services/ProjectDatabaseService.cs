using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;

namespace NRCan.Datahub.Shared.Services
{
    public class ProjectDatabaseService: IProjectDatabaseService
    {
        private readonly string POSTGRES_AZ_RESOURCE = "https://ossrdbms-aad.database.windows.net";

        public async Task<string> GetPostgresToken()
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var token = await tokenProvider.GetAccessTokenAsync(POSTGRES_AZ_RESOURCE);
            return token;
        }

        public async Task<AppAuthenticationResult> GetPostgresAuthenticationObject()
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var token = await tokenProvider.GetAuthenticationResultAsync(POSTGRES_AZ_RESOURCE);
            return token;
        }

        public bool IsServiceAvailable()
        {
            return true;
        }
    }
}
