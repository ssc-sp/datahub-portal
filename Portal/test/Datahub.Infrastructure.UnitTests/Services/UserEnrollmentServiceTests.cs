using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Datahub.Infrastructure.UnitTests.Services;

using static Testing;

public class UserEnrollmentServiceTests
{
    [Test]
    public async Task UserCanEnrollWithDataHubTest()
    {
        SetDatahubGraphInviteFunctionUrl("http://localhost");
        
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = ExpectedDatahubPortalInviteResponse()
            })
            .Verifiable();

        var httpClientFactory = new Mock<IHttpClientFactory>();

        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(
            () => new HttpClient(mockHandler.Object));

        var userEnrollmentService = new UserEnrollmentService(Mock.Of<ILogger<UserEnrollmentService>>(), 
            httpClientFactory.Object, _datahubPortalConfiguration, null);

        var result = await userEnrollmentService.SendUserDatahubPortalInvite(TestUserEmail, default);

        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Is.EqualTo(TestUserGraphGuid));
    }

    [Test]
    public async Task UserCanEnrollWithWhitelistedEmail()
    {
        string[] whiteList = [".gov.au", ".gov.uk"]; 
        SetDatahubGraphInviteFunctionUrl("http://localhost");
        SetAllowedUserEmailDomains(whiteList);
        
        
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = ExpectedDatahubPortalInviteResponse()
            })
            .Verifiable();

        var httpClientFactory = new Mock<IHttpClientFactory>();

        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(
            () => new HttpClient(mockHandler.Object));

        var userEnrollmentService = new UserEnrollmentService(Mock.Of<ILogger<UserEnrollmentService>>(), 
            httpClientFactory.Object, _datahubPortalConfiguration, null);

        var result = await userEnrollmentService.SendUserDatahubPortalInvite(GuestUserEmail, default);

        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Is.EqualTo(TestUserGraphGuid));
    }

    [Test]
    [TestCase("fake@email.com")]
    [TestCase("fake@canada.com")]
    [TestCase("fake@gc.canada.ca")]
    public Task UserCanNotEnrollWithoutAGovernmentEmailTest(string? email)
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _userEnrollmentService.SendUserDatahubPortalInvite(email, default);
        });
        return Task.CompletedTask;
    }

    [Test]
    [Ignore("Needs to be validated")]
    public async Task CanParseResponseJsonProperly()
    {
        var fakeReturnId = Guid.NewGuid().ToString();
        var expectedPullRequestResponse = ExpectedInvitationRequestResponse(fakeReturnId);
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = expectedPullRequestResponse
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);


        var userEnrollmentService = new UserEnrollmentService(Mock.Of<ILogger<UserEnrollmentService>>(),
            httpClientFactory.Object, _datahubPortalConfiguration, null);
        var result = await userEnrollmentService.SendUserDatahubPortalInvite(TestUserEmail, default);

        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Is.EqualTo(fakeReturnId));
    }

    private StringContent ExpectedInvitationRequestResponse(string fakeReturnId, string userEmail = TestUserEmail,
        string groupId = TestProjectAcronym)
    {
        var data = new JsonObject
        {
            ["message"] = $"Successfully invited {userEmail} and added to group {groupId}",
            ["data"] = new JsonObject
            {
                ["email"] = userEmail,
                ["id"] = fakeReturnId
            }
        };
        var stringContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        return stringContent;
    }
}