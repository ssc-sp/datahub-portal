using Datahub.Core.Model.Datahub;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Services.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute;

namespace Datahub.Functions.UnitTests;

namespace Datahub.Functions.UnitTests;

[TestFixture]
public class EmailServiceTests
{
    private EmailService _emailService;
    private DatahubEmailService _datahubEmailService;
    private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private ILogger<EmailService> _mockEmailLogger;
    private ILogger<DatahubEmailService> _mockEmailServiceLogger;
    private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    [SetUp]
    public void Setup()
    {
        _mockEmailLogger = _loggerFactory.CreateLogger<EmailService>();
        _mockEmailServiceLogger = _loggerFactory.CreateLogger<DatahubEmailService>();
        _emailService = new EmailService(_mockEmailLogger);
        _datahubEmailService = new DatahubEmailService(_mockEmailServiceLogger, _publishEndpoint, _dbContextFactory);
    }

    [Test]
    public void BuildEmail_ReturnsEmailRequestMessage_WhenTemplateExists()
    {
        var template = "test_template.html";
        var sendTo = new List<string> { "test@example.com" };
        var bccTo = new List<string> { "bcc@example.com" };
        var bodyArgs = new Dictionary<string, string> { { "{key1}", "value1" } };
        var subjectArgs = new Dictionary<string, string> { { "{key2}", "value2" } };

        var result = _emailService.BuildEmail(template, sendTo, bccTo, bodyArgs, subjectArgs);

        Assert.IsNotNull(result);
        Assert.AreEqual(sendTo, result.To);
        Assert.AreEqual(bccTo, result.BccTo);
    }

    [Test]
    public void BuildEmail_ReturnsNull_WhenTemplateDoesNotExist()
    {
        var template = "nonExistentTemplate";
        var sendTo = new List<string> { "test@example.com" };
        var bccTo = new List<string> { "bcc@example.com" };
        var bodyArgs = new Dictionary<string, string> { { "key1", "value1" } };
        var subjectArgs = new Dictionary<string, string> { { "key2", "value2" } };

        var result = _emailService.BuildEmail(template, sendTo, bccTo, bodyArgs, subjectArgs);

        Assert.IsNull(result);
    }

    [Test]
    public void PopulateTemplate_ReplacesPlaceholders_WithArgsValues()
    {
        var template = "Hello, {name}!";
        var args = new Dictionary<string, string> { { "{name}", "John" } };

        var result = _emailService.PopulateTemplate(template, args);

        Assert.AreEqual("Hello, John!", result);
    }


    [Test]
    public async Task EmailNotificationHandler_ShouldInvokeMassTransit()
    {
        var message = new  
        { 
            to = new List<string> { "test@test.test"}, 
            subject = "test", 
            body = "test" 
        }; 
        await _datahubEmailService.SendToRecipients("test@ssc.gc.ca",message.to,message.subject,message.body);
        Assert.IsTrue(_publishEndpoint.ReceivedCalls().Count() == 1);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _loggerFactory?.Dispose();
    }
}