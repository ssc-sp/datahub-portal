using Datahub.Application.Services.Reports;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages
{
    public record ReportGenerationMessage(string WorkspaceAcronym, WorkspaceReport Report) : IRequest;
}