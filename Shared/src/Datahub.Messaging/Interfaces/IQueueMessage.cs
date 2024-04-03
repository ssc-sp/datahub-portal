using MediatR;

namespace Datahub.Shared.Messaging;

public interface IQueueMessage: IRequest
{
    string Content { get; }
    string ConfigPathOrQueueName { get; }
}
 