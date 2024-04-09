using Datahub.Application.Services.Budget;
using Datahub.Infrastructure.Queues.MessageHandlers;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessageBase
{
    public ProjectUsageUpdateMessageBase(string projectAcronym, List<DailyServiceCost> subCosts, int timeout, bool forceRollover)
    {
        ProjectAcronym = projectAcronym;
        SubscriptionCosts = subCosts;
        Timeout = timeout;
        ForceRollover = forceRollover;
    }

    public string ProjectAcronym { get; }
    public List<DailyServiceCost> SubscriptionCosts { get; }
    public int Timeout { get; }
    public bool ForceRollover { get; set; }
}

public class ProjectUsageUpdateMessage : ProjectUsageUpdateMessageBase, IRequest, IMessageTimeout
{
    public ProjectUsageUpdateMessage(string ProjectAcronym, List<DailyServiceCost> SubscriptionCosts, int Timeout, bool ForceRollover) : base(ProjectAcronym,
        SubscriptionCosts, Timeout, ForceRollover)
    {
    }
}

public class ProjectCapacityUpdateMessage : ProjectUsageUpdateMessageBase, IRequest, IMessageTimeout
{
    public ProjectCapacityUpdateMessage(string projectAcronym, int timeout, bool forceRollover) : base(projectAcronym, new List<DailyServiceCost>(),
        timeout, forceRollover)
    {
    }
}