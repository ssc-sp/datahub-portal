using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Events;

namespace ResourceProvisioner.Domain.Common;

public class BaseUpdateMessage
{
    public List<RepositoryUpdateEvent> Events { get; set; }
    public TerraformWorkspace TerraformWorkspace { get; set; }
}