using MediatR;
using Newtonsoft.Json;
using ResourceProvisioner.Domain.Common;

namespace ResourceProvisioner.Infrastructure.Helpers;

public class ForwardableAdapter<TRequest> : IForwardableMessage where TRequest : IRequest
{
    public TRequest Message { get; private set; }
    public string Content => JsonConvert.SerializeObject(Message);
    public ForwardableAdapter(TRequest message)
    {
        Message = message;
    }
}
