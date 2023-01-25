using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Services;
using Datahub.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Datahub.Infrastructure.UnitTests;

[SetUpFixture]
public partial class Testing
{
    internal static IConfiguration _configuration = null!;
    internal static IUserEnrollmentService _userEnrollmentService = null!;


    internal const string TestProjectAcronym = "TEST";
    internal const string TestUserEmail = "user@email.gc.ca";
    internal const string TestUserId = "123456789";
    internal static readonly string[] TEST_USER_IDS = Enumerable.Range(0,5).Select(_ => new Guid().ToString()).ToArray();
    internal const string TestAdminUserId = "987654321";

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();

        var expectedDatahubPortalInviteResponse = ExpectedDatahubPortalInviteResponse();
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = expectedDatahubPortalInviteResponse
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        
        _userEnrollmentService = new UserEnrollmentService(Mock.Of<ILogger<UserEnrollmentService>>(), _configuration, httpClientFactory.Object);
    }

    private static StringContent ExpectedDatahubPortalInviteResponse()
    {
        var data = new JsonObject
        {
            ["data"] = new JsonObject
            {
                ["id"] = TestUserId
            }
        };
        var stringContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        return stringContent;
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
    }
}