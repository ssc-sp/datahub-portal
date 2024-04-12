using ResourceProvisioner.Domain.Common;
using ResourceProvisioner.Domain.ValueObjects;

namespace ResourceProvisioner.Domain.Messages;

public class PullRequestUpdateMessage : BaseUpdateMessage
{
    public PullRequestValueObject? PullRequestValueObject { get; set; }
}