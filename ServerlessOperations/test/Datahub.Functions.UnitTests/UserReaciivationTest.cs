using Azure.Messaging.ServiceBus;
using Datahub.Core.Model.Context;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using System.Net.Sockets;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class UserReaciivationTest
    {  
        private Mock<IEmailService> _emailServiceMock;
        private IConfiguration _config = Substitute.For<IConfiguration>();
        private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
        private AzureConfig _azureConfig;
        private EmailNotificationHandler _handler;

        [SetUp]
        public void Setup()
        {
            _emailServiceMock = new Mock<IEmailService>(); 
             
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"EmailNotification:IsValid", "true"},
                {"EmailNotification:DumpMessages", "false"},
                {"EmailNotification:SenderName", "Test Sender"},
                {"EmailNotification:SenderAddress", "test@example.com"},
                {"EmailNotification:SmtpHost", "smtp.example.com"},
                {"EmailNotification:SmtpPort", "587"},
                {"EmailNotification:SmtpUsername", "username"},
                {"EmailNotification:SmtpPassword", "password"},
                {"EmailNotification:NotificationsCCAddress", "cc@example.com"}
            };
            var _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _azureConfig = new AzureConfig(_configuration);
            _configuration.Bind("AzureAd", _azureConfig);
            _handler = new EmailNotificationHandler(
                _loggerFactory,
                _azureConfig, 
                _emailServiceMock.Object);
        }

        [Test]
        public async Task Run_ShouldHandleUserReactivationNotification()
        {
            // Arrange
            var emailRequestMessage = new EmailRequestMessage
            {
                To = new List<string> { "user@example.com" },
                Subject = "FSDH Account Reactivation Notification",
                Body = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    { "WorkspaceAcronym", "WSA" },
                    { "LeadName", "John Doe" },
                    { "UserName", "Jane Smith" }
                })
            };

            var messageEnvelope = new
            {
                message = emailRequestMessage
            };
             
            var messageBody = System.Text.Json.JsonSerializer.Serialize(messageEnvelope);
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(
                body: new BinaryData(messageBody));


            _emailServiceMock.Setup(es => es.BuildEmail(
                It.IsAny<string>(),
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<Dictionary<string, string>>()))
                .Returns(emailRequestMessage);

            // Act
            //await _handler.Run(serviceBusReceivedMessage);
            Assert.ThrowsAsync<SocketException>(async () => await _handler.Run(serviceBusReceivedMessage));

            // Assert
            _emailServiceMock.Verify(es => es.BuildEmail(
                "user_reactivation.html",
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}