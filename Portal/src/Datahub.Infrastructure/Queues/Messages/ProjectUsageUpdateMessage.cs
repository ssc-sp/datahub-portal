using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessageBase(
    string projectAcronym,
    string costsBlobName,
    string totalsBlobName,
    bool forceRollover)
{
    public string ProjectAcronym { get; } = projectAcronym;
    public string CostsBlobName { get; } = costsBlobName;
    public string TotalsBlobName { get; set; } = totalsBlobName;
    public bool ForceRollover { get; set; } = forceRollover;
}

public class ProjectUsageUpdateMessage(
    string projectAcronym,
    string costsBlobName,
    string totalsBlobName,
    bool forceRollover)
    : ProjectUsageUpdateMessageBase(projectAcronym,
        costsBlobName, totalsBlobName, forceRollover), IRequest;

public class ProjectCapacityUpdateMessage(string projectAcronym, bool forceRollover) : ProjectUsageUpdateMessageBase(
    projectAcronym, string.Empty, string.Empty,
    forceRollover), IRequest;