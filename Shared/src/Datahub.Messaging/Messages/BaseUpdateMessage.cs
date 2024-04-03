using Datahub.Shared.Entities;

namespace Datahub.Shared.Messaging;

public class BaseUpdateMessage
{
    public List<RepositoryUpdateEvent> Events { get; set; }
    public TerraformWorkspace TerraformWorkspace { get; set; }
}