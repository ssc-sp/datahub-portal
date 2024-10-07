using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    /// <summary>
    /// Represents the different types of bug reports. To create a new type,
    /// add a User Story in ADO and grab the reference ID.
    /// </summary>
    public enum BugReportTypes
    {
        SupportRequest = 1230,
        SystemError = 1700,
        InfrastructureError = 1699,
        PythonWorkspaceSyncError = 7023,
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