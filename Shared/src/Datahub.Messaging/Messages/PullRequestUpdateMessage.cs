using Datahub.Shared.Entities;

namespace Datahub.Shared.Messaging;

public class PullRequestUpdateMessage : BaseUpdateMessage
{
    public PullRequestValueObject? PullRequestValueObject { get; set; }
}