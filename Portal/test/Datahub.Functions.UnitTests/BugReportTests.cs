using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using NSubstitute;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

    [SetUp]
    public void SetUp()
    {
        _logger = _loggerFactory.CreateLogger<BugReport>();
        _azureConfig = new AzureConfig(_config);
        _emailService = new EmailService(_loggerFactory.CreateLogger<EmailService>());
        _bugReport = new BugReport(_logger, _azureConfig, _emailService, _publishEndpoint );
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
    public async Task PublishBugReport_ShouldInvoiceMassTransit()
    {
        QueueMessage qm = QueuesModelFactory.QueueMessage(
            messageId: "bug-report",
            popReceipt: "",
            messageText: JsonSerializer.Serialize(_bugReportMessage),
            dequeueCount: 0);
        await _bugReport.Run(qm);
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
    public async Task CreateIssue_WithValidInputs_ReturnsIssueObject()
    {
        // Act
        var result = await _bugReport.CreateIssue(_bugReportMessage);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<object[]>(result);
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        _loggerFactory?.Dispose();
    }
}