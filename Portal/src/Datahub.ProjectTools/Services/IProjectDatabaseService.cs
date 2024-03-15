using Microsoft.Azure.Services.AppAuthentication;

namespace Datahub.ProjectTools.Services;

public interface IProjectDatabaseService
{
	Task<string> GetPostgresToken();
	Task<AppAuthenticationResult> GetPostgresAuthenticationObject();
	bool IsServiceAvailable { get; }
}