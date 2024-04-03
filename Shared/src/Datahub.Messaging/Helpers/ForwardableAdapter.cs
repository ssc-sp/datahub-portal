using MediatR;
using Newtonsoft.Json;

namespace Datahub.Shared.Messaging;

public class ForwardableAdapter<TRequest> : IForwardableMessage where TRequest : IRequest
{
    public TRequest Message { get; private set; }
    public string Content => JsonConvert.SerializeObject(Message);
    public ForwardableAdapter(TRequest message)
    {
        Message = message;
    }
}
