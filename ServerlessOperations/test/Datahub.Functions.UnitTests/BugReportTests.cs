using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NSubstitute;

namespace Datahub.Functions.UnitTests;

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
        var httpClient = new HttpClient();

        
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        _azureConfig = new AzureConfig(_config);
        _emailService = new EmailService(_loggerFactory.CreateLogger<EmailService>());
        _alertRecordService = new AlertRecordService(_azureConfig);
        
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
    [Ignore("Need to fix")]
    public void BuildEmail_WithValidInputs_ReturnsEmailRequestMessage()
    {
        // Arrange
        var response = new WorkItem()
        {
            Id = 0,
            Url = "Test Url"
        };

        // Act
        // var result = _bugReport.BuildEmail(_bugReportMessage, response);

        // Assert
        // Assert.IsNotNull(result);
        // Assert.AreEqual(_azureConfig.Email.AdminEmail, result.To[0]);
        // Assert.AreEqual("bug_report.html", result.Template);
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
}