using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Data.Databricks;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Notebooks;
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
    internal static DatabricksApiService _databricksApiService = null!;
    internal static DatahubPortalConfiguration _datahubPortalConfiguration = null!;
    internal static DatahubProjectDBContext _dbContext =
                new DatahubProjectDBContext(new DbContextOptions<DatahubProjectDBContext>());
    internal static IDbContextFactory<DatahubProjectDBContext> _dbContextFactory =
        Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
    internal static ILoggerFactory _loggerFactory;
    internal static IHttpClientFactory _httpClientFactory = null;
    internal static Mock<HttpMessageHandler> _mockHandler = null;

    internal const string TestProjectAcronym = "TEST";
    internal const string TestUserEmail = "user@email.gc.ca";
    internal const string TestUserGraphGuid = "0000-0000-0000-0000-0000";
    internal static readonly string[] TEST_USER_IDS = Enumerable.Range(0,5).Select(_ => Guid.NewGuid().ToString()).ToArray();
    internal const string TestAdminUserId = "987654321";
    internal const string TestResourceGroupId = "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg";
    internal const string SubscriptionId = "bc4bcb08-d617-49f4-b6af-69d6f10c240b";
    internal const string SubscriptionResourceId = "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b";
    internal const string TestBudgetId = "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Consumption/budgets/fsdh-test-budget";
    internal const string TestStorageAccountId = "/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh-static-test-rg/providers/Microsoft.Storage/storageAccounts/fsdhteststorageaccount";
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
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        var expectedDatahubPortalInviteResponse = ExpectedDatahubPortalInviteResponse();

        _mockHandler = new Mock<HttpMessageHandler>();
        _mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = expectedDatahubPortalInviteResponse
            })
            .Verifiable();

        var httpClientFactory = new Mock<IHttpClientFactory>();

        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(
            () => new HttpClient(_mockHandler.Object));

        _httpClientFactory = httpClientFactory.Object;

        _userEnrollmentService = new UserEnrollmentService(Mock.Of<ILogger<UserEnrollmentService>>(), 
            httpClientFactory.Object, _datahubPortalConfiguration, null);

        _databricksApiService = new DatabricksApiService(Mock.Of<ILogger<DatabricksApiService>>(), 
            httpClientFactory.Object, _dbContextFactory, Mock.Of<IDatahubCatalogSearch>());
    }

    public static void SetDatahubGraphInviteFunctionUrl(string value)
    {
        _datahubPortalConfiguration.DatahubGraphInviteFunctionUrl = value;
    }
    public static void SetAllowedUserEmailDomains(string[] value)
    {
        _datahubPortalConfiguration.AllowedUserEmailDomains = value;
    }

    public static void SetDatabricksApiService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _databricksApiService = new DatabricksApiService(Mock.Of<ILogger<DatabricksApiService>>(),
            _httpClientFactory, dbContextFactory, Mock.Of<IDatahubCatalogSearch>());
    }
    public static StringContent ExpectedDatahubPortalInviteResponse()
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

    internal static StringContent ExpectedDatabricksAddUserResponse()
    {
        var databricksUser = new DatabricksUser
        {
            userName = TestUserEmail,
            name = new Name { familyName = TEST_USER_IDS[0] },
            id = TestAdminUserId,
            active = true,
            emails =
            [
                new Email { primary = true, value = TestUserEmail, type = "work", display=TEST_USER_IDS[0] }
            ]
        };
        var stringContent = new StringContent(JsonSerializer.Serialize(databricksUser), Encoding.UTF8, "application/json");
        return stringContent;
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        _loggerFactory.Dispose();
        _dbContext?.Dispose();
    }
}