using System.Net;

namespace ResourceProvisioner.SpecflowTests;

public class TestHttpMessageHandler : HttpMessageHandler
{
    
    private readonly Queue<HttpResponseMessage> _responses = new();
    
    public void AddResponse(HttpResponseMessage response)
    {
        _responses.Enqueue(response);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_responses.Dequeue());
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _responses.Dequeue();
    }
}