using ResourceProvisioner.Domain.Common;
using ResourceProvisioner.Domain.Enums;

namespace ResourceProvisioner.Domain.Events;

public class RepositoryUpdateEvent : BaseEvent
{
    public MessageStatusCode StatusCode { get; set; }
    public string Message { get; set; }
}