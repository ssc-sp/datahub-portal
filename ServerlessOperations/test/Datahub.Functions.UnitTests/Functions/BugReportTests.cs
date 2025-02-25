using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Datahub.Functions.Entities;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq.Protected;
using Moq;
using NSubstitute;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Datahub.Functions.UnitTests.Functions;

[TestFixture]
public class BugReportTests
{
    private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private IMediator _mediator = Substitute.For<IMediator>();
    private IConfiguration _config = Substitute.For<IConfiguration>();

    private BugReport _bugReport;
    private ILogger<BugReport> _logger;
    private AzureConfig _azureConfig;
    private IEmailService _emailService;
    private BugReportMessage _bugReportMessage;
    private ISendEndpointProvider _iSendEndpointProvider;
    private IAlertRecordService _alertRecordService;

    [SetUp]
    public void SetUp()
    {
        _iSendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        _logger = _loggerFactory.CreateLogger<BugReport>();

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
 
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Success"),
            });
         
        var httpClient = new HttpClient(handlerMock.Object);

        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        _config["BugReportTeamsWebhookUrl"].Returns("https://fake_webhook_url.tld");
        _azureConfig = new AzureConfig(_config);

        _emailService = new EmailService(_loggerFactory.CreateLogger<EmailService>());
 
        _alertRecordService = Substitute.For<IAlertRecordService>();

        _bugReport = new BugReport(_logger, _azureConfig, _emailService, _iSendEndpointProvider, _alertRecordService, httpClientFactory);
        _bugReportMessage = new BugReportMessage(
            UserName: "Test",
            UserEmail: "example@email.com",
            UserOrganization: "ssc-spc",
            PortalLanguage: "en",
            PreferredLanguage: "en",
            Timezone: "EST",
            Workspaces: "DIE1",
            Topics: "Test",
            URL: "google.com",
            UserAgent: "test",
            Resolution: "1920x1080",
            LocalStorage: "{}",
            BugReportType: BugReportTypes.SupportRequest,
            Description: "Test report"
        );
    }

    [Test]
    public void BuildEmail_WithValidInputs_ReturnsEmailRequestMessage()
    {
        // Arrange
        var response = new WorkItem()
        {
            Id = 0,
            Url = "Test Url"
        };

        // Act
        var result = _bugReport.BuildEmail(_bugReportMessage, response);

        // Assert
        result.Should().NotBeNull();
        result.To[0].Should().Be(_azureConfig.Email.AdminEmail);
        result.Template.Should().Be("bug_report.html");
    }

    [Test]
    [Ignore("Need to fix")]
    public void CreateIssue_WithValidInputs_ReturnsIssueObject()
    {
        // Act
        var result = _bugReport.CreateIssue(_bugReportMessage);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<object[]>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _loggerFactory?.Dispose();
    }

    [Test]
    public void Run_WithValidMessage_ReturnsSuccess()
    {
        // Arrange

        var messageEnvelope = new JsonObject
        {
            ["message"] = JsonSerializer.SerializeToNode(_bugReportMessage)
        };
        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(
            new AmqpAnnotatedMessage(new AmqpMessageBody(new List<ReadOnlyMemory<byte>>
            {
                Encoding.UTF8.GetBytes(messageEnvelope.ToJsonString())
            })), new BinaryData("lockToken"u8.ToArray()));
 
        _alertRecordService.GetRecentAlertForBugMessage(Arg.Any<BugReportMessage>())
            .Returns(Task.FromResult(new ReceivedAlert { EmailSent=true }));



        // Act
        var result = _bugReport.Run(serviceBusReceivedMessage);

        // Assert
        result.Should().NotBeNull();
        //result.To[0].Should().Be(_azureConfig.Email.AdminEmail);
        //result.Template.Should().Be("bug_report.html");
    }

    [Test]
    public void Run_WithNewInfrastructureError_ReturnsSuccess()
    {
        // Arrange
        var bugInfrastructureErrorMessage = new BugReportMessage(
            UserName: "Test",
            UserEmail: "example@email.com",
            UserOrganization: "ssc-spc",
            PortalLanguage: "en",
            PreferredLanguage: "en",
            Timezone: "EST",
            Workspaces: "DIE1",
            Topics: "Test",
            URL: "google.com",
            UserAgent: "test",
            Resolution: "1920x1080",
            LocalStorage: "{}",
            BugReportType: BugReportTypes.InfrastructureError,
            Description: "Test report"
        );

        var messageEnvelope = new JsonObject
        {
            ["message"] = JsonSerializer.SerializeToNode(bugInfrastructureErrorMessage)
        };
        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(
            new AmqpAnnotatedMessage(new AmqpMessageBody(new List<ReadOnlyMemory<byte>>
            {
                Encoding.UTF8.GetBytes(messageEnvelope.ToJsonString())
            })), new BinaryData("lockToken"u8.ToArray()));

        _alertRecordService.GetRecentAlertForBugMessage(Arg.Any<BugReportMessage>())
            .Returns(Task.FromResult(new ReceivedAlert { EmailSent = false }));

        // Act
        var result = _bugReport.Run(serviceBusReceivedMessage);

        // Assert
        result.Should().NotBeNull();
        // Failing to post the issue to ADO.
        // TODO: continue tests after TODO in BugReport.cs is done - enable configuration for _postToDevops 
    }

    [Test]
    public void Run_WithInfrastructureErrorAlreadySent_ReturnsSuccess()
    {
        // Arrange
        var bugInfrastructureErrorMessage = new BugReportMessage(
            UserName: "Test",
            UserEmail: "example@email.com",
            UserOrganization: "ssc-spc",
            PortalLanguage: "en",
            PreferredLanguage: "en",
            Timezone: "EST",
            Workspaces: "DIE1",
            Topics: "Test",
            URL: "google.com",
            UserAgent: "test",
            Resolution: "1920x1080",
            LocalStorage: "{}",
            BugReportType: BugReportTypes.InfrastructureError,
            Description: "Test report"
        );
 
        var messageEnvelope = new JsonObject
        {
            ["message"] = JsonSerializer.SerializeToNode(bugInfrastructureErrorMessage)
        };
        var serviceBusReceivedMessage = ServiceBusReceivedMessage.FromAmqpMessage(
            new AmqpAnnotatedMessage(new AmqpMessageBody(new List<ReadOnlyMemory<byte>>
            {
                Encoding.UTF8.GetBytes(messageEnvelope.ToJsonString())
            })), new BinaryData("lockToken"u8.ToArray()));

        _alertRecordService.GetRecentAlertForBugMessage(Arg.Any<BugReportMessage>())
            .Returns(Task.FromResult(new ReceivedAlert { EmailSent = true }));

        // Act
        var result = _bugReport.Run(serviceBusReceivedMessage);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TaskStatus.RanToCompletion); 
    }
}