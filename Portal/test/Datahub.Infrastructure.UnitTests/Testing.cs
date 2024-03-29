using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests;

[SetUpFixture]
public partial class Testing
{
    internal static IConfiguration _configuration = null!;
    internal static IUserEnrollmentService _userEnrollmentService = null!;
    internal static DatahubPortalConfiguration _datahubPortalConfiguration = null!;
    internal static IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = null!;


    internal const string TestProjectAcronym = "TEST";
    internal const string TestUserEmail = "user@email.gc.ca";
    internal const string TestUserGraphGuid = "0000-0000-0000-0000-0000";
    internal static readonly string[] TEST_USER_IDS = Enumerable.Range(0,5).Select(_ => Guid.NewGuid().ToString()).ToArray();
    internal const string TestAdminUserId = "987654321";
    internal const string OldUserEmail = "old-user@email.gc.ca";
    internal const string GuestUserEmail = "user@email.gov.uk";
    internal const string OldUserId = "987654321";

    internal const string TestWebAppId =
        "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourcegroups/fsdh-static-test-rg/providers/Microsoft.Web/sites/fsdh-dev-test-web-app";
    internal static readonly string[] PROJECT_ACRONYMS = Enumerable.Range(1, 3).Select(i => $"TEST{i}").ToArray();
    internal static readonly string[] PROJECT_NAMES = Enumerable.Range(1, 3).Select(i => $"Test Project {i}").ToArray();
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
            .Build();
        
        _datahubPortalConfiguration = new DatahubPortalConfiguration();
        _configuration.Bind(_datahubPortalConfiguration);
        _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();

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

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(
            () => new HttpClient(mockHandler.Object));
        
        _userEnrollmentService = new UserEnrollmentService(Mock.Of<ILogger<UserEnrollmentService>>(), httpClientFactory.Object, _datahubPortalConfiguration, null);
    }

    public static void SetDatahubGraphInviteFunctionUrl(string value)
    {
        _datahubPortalConfiguration.DatahubGraphInviteFunctionUrl = value;
    }
    public static void SetAllowedUserEmailDomains(string[] value)
    {
        _datahubPortalConfiguration.AllowedUserEmailDomains = value;
    }
    private static StringContent ExpectedDatahubPortalInviteResponse()
    {
        var data = new JsonObject
        {
            ["data"] = new JsonObject
            {
                ["id"] = TestUserGraphGuid
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