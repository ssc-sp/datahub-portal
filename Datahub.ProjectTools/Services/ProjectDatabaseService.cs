using System.Threading.Tasks;
using Datahub.Core.Services.Resources;
using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.ProjectTools.Services
{
	public class ProjectDatabaseService : IProjectDatabaseService
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

		public bool IsServiceAvailable => true;
	}
}
