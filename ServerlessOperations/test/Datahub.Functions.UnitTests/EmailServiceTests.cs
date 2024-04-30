using Datahub.Functions.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Datahub.Functions.UnitTests;

[TestFixture]
public class EmailServiceTests
{
    private Mock<ILogger<EmailService>> _mockLogger;
    private EmailService _emailService;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(_mockLogger.Object);
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
        
        result.Should().NotBeNull();
        result!.To.Should().BeEquivalentTo(sendTo);
        result!.BccTo.Should().BeEquivalentTo(bccTo);
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

        result.Should().BeNull();
    }

    [Test]
    public void PopulateTemplate_ReplacesPlaceholders_WithArgsValues()
    {
        var template = "Hello, {name}!";
        var args = new Dictionary<string, string> { { "{name}", "John" } };

        var result = _emailService.PopulateTemplate(template, args);

        result.Should().NotBeNull();
        result.Should().Be("Hello, John!");
    }
}