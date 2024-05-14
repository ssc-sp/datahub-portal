using Datahub.Application.Services.Budget;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessageBase(string projectAcronym, List<DailyServiceCost> subCosts, bool forceRollover)
{
    public string ProjectAcronym { get; } = projectAcronym;
    public List<DailyServiceCost> SubscriptionCosts { get; } = subCosts;
    public bool ForceRollover { get; set; } = forceRollover;
}

public class ProjectUsageUpdateMessage(
    string projectAcronym,
    List<DailyServiceCost> subscriptionCosts,
    bool forceRollover)
    : ProjectUsageUpdateMessageBase(projectAcronym,
        subscriptionCosts, forceRollover), IRequest;

public class ProjectCapacityUpdateMessage(string projectAcronym, bool forceRollover) : ProjectUsageUpdateMessageBase(
    projectAcronym, [],
    forceRollover), IRequest;