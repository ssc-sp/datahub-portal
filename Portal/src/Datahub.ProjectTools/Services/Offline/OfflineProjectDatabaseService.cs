using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.ProjectTools.Services.Offline;

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

	public bool IsServiceAvailable => false;
}