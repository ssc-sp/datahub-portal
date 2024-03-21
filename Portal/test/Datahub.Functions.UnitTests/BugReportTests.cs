using Azure.Storage.Queues.Models;
using Datahub.Application.Services.Security;
using Datahub.Core.Services.Security;
using Datahub.Functions;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Security;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using JsonSerializer = System.Text.Json.JsonSerializer;

[TestFixture]
public class BugReportTests
{
    private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private IKeyVaultService _keyVaultService = Substitute.For<IKeyVaultService>();
    private IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
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
            userName: "Test",
            userEmail: "example@email.com",
            userOrganization: "ssc-spc",
            portalLanguage: "en",
            preferredLanguage: "en",
            timezone: "EST",
            workspaces: "DIE1",
            topics: "Test",
            url: "google.com",
            userAgent: "test",
            resolution: "1920x1080",
            localStorage: "{}",
            bugReportType: BugReportTypes.SupportRequest,
            description: "Test report"
        );
    }

    [Test]
    [Ignore("Need to fix")]
    public async Task PublishBugReport_ShouldInvokeMassTransit()
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
        var response = new Dictionary<string, object>
        {
            { "url", "Test Url" },
            { "id", "Test Id" }
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