using Datahub.Application.Services.Budget;
using Datahub.Infrastructure.Queues.MessageHandlers;
using MediatR;

namespace Datahub.Infrastructure.Queues.Messages;

public class ProjectUsageUpdateMessageBase
{
    public ProjectUsageUpdateMessageBase(string projectAcronym, List<DailyServiceCost> subCosts, int timeout)
    {
        ProjectAcronym = projectAcronym;
        SubscriptionCosts = subCosts;
        Timeout = timeout;
    }

    public string ProjectAcronym { get; }
    public List<DailyServiceCost> SubscriptionCosts { get; }
    public int Timeout { get; }
}

public class ProjectUsageUpdateMessage : ProjectUsageUpdateMessageBase, IRequest, IMessageTimeout
{
    public ProjectUsageUpdateMessage(string projectAcronym, List<DailyServiceCost> subCosts, int timeout) : base(projectAcronym,
        subCosts, timeout)
    {
    }
}

public class ProjectCapacityUpdateMessage : ProjectUsageUpdateMessageBase, IRequest, IMessageTimeout
{
    public ProjectCapacityUpdateMessage(string projectAcronym, int timeout) : base(projectAcronym, new List<DailyServiceCost>(),
        timeout)
    {
    }
}