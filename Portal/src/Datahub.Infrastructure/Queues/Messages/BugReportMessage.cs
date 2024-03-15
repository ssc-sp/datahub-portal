using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
	public enum BugReportTypes
	{
		SupportRequest = 1230,
		SystemError = 1700,
		InfrastructureError = 1699
	}
	public record BugReportMessage(
		string? UserName,
		string? UserEmail,
		string? UserOrganization,
		string? PortalLanguage,
		string? PreferredLanguage,
		string? Timezone,
		string? Workspaces,
		string? Topics,
		string? URL,
		string? UserAgent,
		string? Resolution,
		string? LocalStorage,
		BugReportTypes BugReportType,
		string Description
	) : IRequest;
}