using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Json;
using Moq;
using System.Text;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using NSubstitute;
using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Azure.Core.Serialization;
using Microsoft.Extensions.Options;

namespace Datahub.Functions.UnitTests
{
    public static class TestHelper
    {

        /// <summary>
        /// Mocking GraphServiceClient
        /// based on https://medium.com/@carlosedgarnovo_56347/unit-testing-microsoft-graphserviceclient-in-c-net-d86a33e9158b
        /// </summary>
        /// <returns>Mock GraphServiceClient</returns>
        public static GraphServiceClient MockGraphServiceClient()
        {
            Mock<IRequestAdapter> _requestAdapterMock = new();
            Mock<ISerializationWriterFactory> _serializationWriterFactoryMock = new();

            _serializationWriterFactoryMock.Setup(factory => factory.GetSerializationWriter(It.IsAny<string>())).Returns(new JsonSerializationWriter());

            _requestAdapterMock.SetupGet(adapter => adapter.BaseUrl).Returns("http://graph.test.internal/mock");
            _requestAdapterMock.SetupSet(adapter => adapter.BaseUrl = It.IsAny<string>());
            _requestAdapterMock.Setup(adapter => adapter.EnableBackingStore(It.IsAny<IBackingStoreFactory>()));
            _requestAdapterMock.SetupGet(adapter => adapter.SerializationWriterFactory).Returns(_serializationWriterFactoryMock.Object);

            // Initializing the GraphServiceClient using the mocked request adapter
            return new GraphServiceClient(_requestAdapterMock.Object);
        }

        public static HttpRequestData CreateHttpRequestData(string requestBody)
        {
            var context = Substitute.For<FunctionContext>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Options.Create(new WorkerOptions
            {
                Serializer = new JsonObjectSerializer()
            }));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            context.InstanceServices.Returns(serviceProvider);

            return new FakeHttpRequestData(context, new Uri("http://localhost"), new MemoryStream(Encoding.UTF8.GetBytes(requestBody)));
        }

        public class FakeHttpRequestData : HttpRequestData
        {
            public FakeHttpRequestData(FunctionContext functionContext, Uri url, Stream body = null) : base(functionContext)
            {
                Url = url;
                Body = body ?? new MemoryStream();
            }
            public override Stream Body { get; } = new MemoryStream();
            public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();
            public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
            public override Uri Url { get; }
            public override IEnumerable<ClaimsIdentity> Identities { get; }
            public override string Method { get; }
            public override HttpResponseData CreateResponse()
            {
                return new FakeHttpResponseData(FunctionContext);
            }
        }
        public class FakeHttpResponseData : HttpResponseData
        {
            public FakeHttpResponseData(FunctionContext functionContext) : base(functionContext)
            {
            }

            public override HttpStatusCode StatusCode { get; set; }
            public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
            public override Stream Body { get; set; } = new MemoryStream();
            public override HttpCookies Cookies { get; }

            public async Task WriteAsJsonAsync<T>(T content)
            {
                await Task.CompletedTask;
            }

        }
    }
}
