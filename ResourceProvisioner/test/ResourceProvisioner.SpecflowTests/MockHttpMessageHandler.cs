using System.Net;

namespace ResourceProvisioner.SpecflowTests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public HttpResponseMessage Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Response;
    }
}