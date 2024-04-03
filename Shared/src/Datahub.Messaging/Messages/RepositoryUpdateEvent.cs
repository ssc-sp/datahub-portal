namespace Datahub.Shared.Messaging;
public class RepositoryUpdateEvent : BaseEvent
{
    public MessageStatusCode StatusCode { get; set; }
    public string Message { get; set; }
}