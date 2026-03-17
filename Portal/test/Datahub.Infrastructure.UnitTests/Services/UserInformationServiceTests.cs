using System.Net;
using System.Reflection;
using System.Text;
using Azure.Core;
using Datahub.Application.Services;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services.UserManagement;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Graph;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services;

public class UserInformationServiceTests
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<DatahubProjectDBContext> _dbOptions = null!;
    private Mock<IDbContextFactory<DatahubProjectDBContext>> _dbFactoryMock = null!;
    private Mock<IDatahubCatalogSearch> _catalogSearchMock = null!;
    private Mock<IUserEnrollmentService> _userEnrollmentServiceMock = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public async Task SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _dbOptions = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseSqlite(_connection)
            .Options;

        await using (var initContext = new SqliteDatahubContext(_dbOptions))
        {
            await initContext.Database.EnsureCreatedAsync();
        }

        _dbFactoryMock = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _dbFactoryMock
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new SqliteDatahubContext(_dbOptions));

        _catalogSearchMock = new Mock<IDatahubCatalogSearch>();
        _catalogSearchMock
            .Setup(s => s.AddCatalogObject(It.IsAny<Core.Model.Catalog.CatalogObject>()))
            .Returns(Task.CompletedTask);

        _userEnrollmentServiceMock = new Mock<IUserEnrollmentService>();
        _userEnrollmentServiceMock
            .Setup(s => s.InviteUserToGroup(It.IsAny<string>()))
            .ReturnsAsync("ok");

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:TenantId"] = "11111111-1111-1111-1111-111111111111",
                ["AzureAd:ClientId"] = "22222222-2222-2222-2222-222222222222",
                ["AzureAd:ClientSecret"] = "unit-test-secret"
            })
            .Build();
    }

    [TearDown]
    public void TearDown()
    {
        _connection.Dispose();
    }

    [Test]
    public async Task CreatePortalExternalUserAsync_CreatesPortalAndExternalUsers()
    {
        var sut = BuildService(CreateGraphClientForUser("entra-user-1", "entra.user@test.gc.ca", "Entra User", "Data"));

        var result = await sut.CreatePortalExternalUserAsync(
            "external-oid-1",
            "Ada",
            "Lovelace",
            "SSC",
            "ada.lovelace@example.com",
            DateTimeOffset.UtcNow.AddDays(30));

        Assert.That(result, Is.Not.Null);

        await using var ctx = new SqliteDatahubContext(_dbOptions);
        var portal = await ctx.PortalUsers.Include(p => p.ExternalUser).SingleAsync();
        var external = await ctx.ExternalUsers.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(portal.Email, Is.EqualTo("ada.lovelace@example.com"));
            Assert.That(portal.ExternalUserId, Is.EqualTo(external.Id));
            Assert.That(external.PortalUserId, Is.EqualTo(portal.Id));
            Assert.That(external.ExternalSubject, Is.EqualTo("external-oid-1"));
        });

        _catalogSearchMock.Verify(s => s.AddCatalogObject(It.IsAny<Core.Model.Catalog.CatalogObject>()), Times.Once);
        _userEnrollmentServiceMock.Verify(s => s.InviteUserToGroup(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreatePortalExternalUserAsync_ReturnsNullWhenExternalExists()
    {
        await SeedExistingExternalUser("external-oid-existing", "existing@example.com");
        var sut = BuildService(CreateGraphClientForUser("entra-user-1", "entra.user@test.gc.ca", "Entra User", "Data"));

        var result = await sut.CreatePortalExternalUserAsync(
            "external-oid-existing",
            "New",
            "User",
            "SSC",
            "new.user@example.com",
            DateTimeOffset.UtcNow.AddDays(10));

        Assert.That(result, Is.Null);

        await using var ctx = new SqliteDatahubContext(_dbOptions);
        Assert.That(await ctx.ExternalUsers.CountAsync(), Is.EqualTo(1));
        Assert.That(await ctx.PortalUsers.CountAsync(), Is.EqualTo(1));

        _catalogSearchMock.Verify(s => s.AddCatalogObject(It.IsAny<Core.Model.Catalog.CatalogObject>()), Times.Never);
    }

    [Test]
    public async Task CreatePortalEntraUserAsync_CreatesPortalAndEntraUsers()
    {
        const string graphId = "entra-graph-id-1";
        var sut = BuildService(CreateGraphClientForUser(graphId, "entra.user@example.gc.ca", "Entra User", "Digital"));

        var result = await sut.CreatePortalEntraUserAsync(graphId);

        Assert.That(result, Is.Not.Null);

        await using var ctx = new SqliteDatahubContext(_dbOptions);
        var portal = await ctx.PortalUsers.Include(p => p.EntraUser).SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(portal.Email, Is.EqualTo("entra.user@example.gc.ca"));
            Assert.That(portal.DisplayName, Is.EqualTo("Entra User"));
            Assert.That(portal.EntraUser, Is.Not.Null);
            Assert.That(portal.EntraUser!.GraphGuid, Is.EqualTo(graphId));
        });

        _catalogSearchMock.Verify(s => s.AddCatalogObject(It.IsAny<Core.Model.Catalog.CatalogObject>()), Times.Once);
        _userEnrollmentServiceMock.Verify(s => s.InviteUserToGroup(graphId), Times.Once);
    }

    [Test]
    public async Task CreatePortalEntraUserAsync_ReturnsNullWhenEntraExists()
    {
        const string graphId = "entra-existing-id";
        await SeedExistingEntraUser(graphId, "existing.entra@example.gc.ca");
        var sut = BuildService(CreateGraphClientForUser(graphId, "ignored@example.gc.ca", "Ignored User", "Ignored"));

        var result = await sut.CreatePortalEntraUserAsync(graphId);

        Assert.That(result, Is.Null);

        await using var ctx = new SqliteDatahubContext(_dbOptions);
        Assert.That(await ctx.EntraUsers.CountAsync(), Is.EqualTo(1));
        Assert.That(await ctx.PortalUsers.CountAsync(), Is.EqualTo(1));

        _catalogSearchMock.Verify(s => s.AddCatalogObject(It.IsAny<Core.Model.Catalog.CatalogObject>()), Times.Never);
        _userEnrollmentServiceMock.Verify(s => s.InviteUserToGroup(It.IsAny<string>()), Times.Never);
    }

    private UserInformationService BuildService(GraphServiceClient graphClient)
    {
        var featureManagerMock = new Mock<IFeatureManagerSnapshot>();
        featureManagerMock
            .Setup(f => f.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var authStateProviderMock = new Mock<AuthenticationStateProvider>();
        var serviceAuthManagerMock = new Mock<IServiceAuthManager>();

        var sut = new UserInformationService(
            Mock.Of<ILogger<UserInformationService>>(),
            authStateProviderMock.Object,
            _configuration,
            serviceAuthManagerMock.Object,
            _catalogSearchMock.Object,
            featureManagerMock.Object,
            _userEnrollmentServiceMock.Object,
            _dbFactoryMock.Object);

        var graphField = typeof(UserInformationService)
            .GetField("graphServiceClient", BindingFlags.Instance | BindingFlags.NonPublic);

        graphField!.SetValue(sut, graphClient);
        return sut;
    }

    private async Task SeedExistingExternalUser(string externalSubject, string email)
    {
        await using var ctx = new SqliteDatahubContext(_dbOptions);

        var portalUser = new PortalUser
        {
            Email = email,
            DisplayName = "Existing External"
        };

        ctx.PortalUsers.Add(portalUser);
        await ctx.SaveChangesAsync();

        var externalUser = new ExternalUser
        {
            ExternalSubject = externalSubject,
            FirstName = "Existing",
            LastName = "User",
            Organization = "SSC",
            UserExpiryDate = DateTimeOffset.UtcNow.AddDays(30),
            PortalUser = portalUser,
            PortalUserId = portalUser.Id
        };

        ctx.ExternalUsers.Add(externalUser);
        await ctx.SaveChangesAsync();

        portalUser.ExternalUserId = externalUser.Id;
        await ctx.SaveChangesAsync();
    }

    private async Task SeedExistingEntraUser(string graphId, string email)
    {
        await using var ctx = new SqliteDatahubContext(_dbOptions);

        var portalUser = new PortalUser
        {
            Email = email,
            DisplayName = "Existing Entra",
            EntraUser = new EntraUser
            {
                GraphGuid = graphId,
                PortalUser = null!
            }
        };

        ctx.PortalUsers.Add(portalUser);
        await ctx.SaveChangesAsync();
    }

    private static GraphServiceClient CreateGraphClientForUser(string graphId, string mail, string displayName, string department)
    {
        var handler = new TestGraphHandler(_ =>
        {
            var body = $$"""
                         {
                           "id": "{{graphId}}",
                           "mail": "{{mail}}",
                           "displayName": "{{displayName}}",
                           "department": "{{department}}"
                         }
                         """;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        return new GraphServiceClient(httpClient, new TestTokenCredential());
    }

    private sealed class TestTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new("token", DateTimeOffset.UtcNow.AddHours(1));

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(new AccessToken("token", DateTimeOffset.UtcNow.AddHours(1)));
    }

    private sealed class TestGraphHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responseFactory(request));
    }
}
