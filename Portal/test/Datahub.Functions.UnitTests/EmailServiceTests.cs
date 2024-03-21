﻿using Moq;
using Datahub.Functions.Services;
using Microsoft.Extensions.Logging;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Functions;
using Datahub.Infrastructure.Services;
using Newtonsoft.Json;
using NSubstitute;
using Microsoft.Extensions.Configuration;
using MassTransit;

namespace Datahub.Functions.UnitTests;

[TestFixture]
public class EmailServiceTests
{
    private Mock<ILogger<EmailService>> _mockLogger;
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private IConfiguration _config = Substitute.For<IConfiguration>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private AzureConfig _azureConfig;
    private EmailService _emailService;
    private QueuePongService _pongService;
    private EmailNotificationSender _notificationHandler;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_mockLogger.Object);
        _azureConfig = new AzureConfig(_config);
        _pongService = new QueuePongService(_publishEndpoint);
        _notificationHandler = new EmailNotificationSender(_loggerFactory, _azureConfig, _publishEndpoint, _pongService);
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
        var request = JsonConvert.SerializeObject(message);
        await _notificationHandler.Run(request);
        Assert.IsTrue(_publishEndpoint.ReceivedCalls().Count() == 1);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _loggerFactory?.Dispose();
    }
}