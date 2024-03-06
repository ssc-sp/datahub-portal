using Moq;
using Datahub.Functions.Services;
using Microsoft.Extensions.Logging;

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
    public void BuildEmailReturnsEmailRequestMessageWhenTemplateExists()
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
    public void BuildEmailReturnsNullWhenTemplateDoesNotExist()
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
    public void PopulateTemplateReplacesPlaceholdersWithArgsValues()
    {
        var template = "Hello, {name}!";
        var args = new Dictionary<string, string> { { "{name}", "John" } };

        var result = _emailService.PopulateTemplate(template, args);

        Assert.AreEqual("Hello, John!", result);
    }
}