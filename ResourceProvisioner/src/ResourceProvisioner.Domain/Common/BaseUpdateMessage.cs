using ResourceProvisioner.Domain.Entities;
using ResourceProvisioner.Domain.Events;

namespace ResourceProvisioner.Domain.Common;

public class BaseUpdateMessage
{
    public List<RepositoryUpdateEvent> Events { get; set; }
    public Workspace Workspace { get; set; }
}