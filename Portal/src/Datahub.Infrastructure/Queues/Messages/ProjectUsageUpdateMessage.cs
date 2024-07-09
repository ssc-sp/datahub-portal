using Datahub.Application.Services.Budget;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessageBase(string projectAcronym, string costsBlobName, bool forceRollover)
{
    public string ProjectAcronym { get; } = projectAcronym;
    public string CostsBlobName { get; } = costsBlobName;
    public bool ForceRollover { get; set; } = forceRollover;
}

public class ProjectUsageUpdateMessage(
    string projectAcronym,
    string costsBlobName,
    bool forceRollover)
    : ProjectUsageUpdateMessageBase(projectAcronym,
        costsBlobName, forceRollover), IRequest;

public class ProjectCapacityUpdateMessage(string projectAcronym, bool forceRollover) : ProjectUsageUpdateMessageBase(
    projectAcronym, string.Empty,
    forceRollover), IRequest;