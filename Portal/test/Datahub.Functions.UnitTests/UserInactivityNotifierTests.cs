using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Functions.UnitTests
{
    public class UserInactivityNotifierTests
    {
        private UserInactivityNotifier _sut;

        private readonly IDateProvider _dateProvider = Substitute.For<IDateProvider>();
        private readonly IMediator _mediator = Substitute.For<IMediator>();
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory =
            Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();

        private readonly IUserInactivityNotificationService _userInactivityNotificationService =
            Substitute.For<IUserInactivityNotificationService>();

        private readonly IConfiguration _config = Substitute.For<IConfiguration>();


        private AzureConfig _azConfig;
        private QueuePongService _pongService;
        private EmailValidator _emailValidator;
        private IEmailService _emailService;

        [SetUp]
        public void Setup()
        {
            _azConfig = new AzureConfig(_config);
            _pongService = new QueuePongService(_mediator);
            _emailValidator = new EmailValidator();
            _emailService = new EmailService(_loggerFactory.CreateLogger<EmailService>());
            _sut = new UserInactivityNotifier(_mediator, _loggerFactory, _dbContextFactory, _dateProvider, _azConfig,
                _pongService, _emailValidator, _userInactivityNotificationService, _emailService);
        }

        [Test]
        [TestCase(10, new[] { 10, 2 })]
        [TestCase(1, new[] { 1, 0 })]
        [TestCase(0, new[] { 0, 0, 100 })]
        public async Task CheckIfUserToBeNotifiedInLockedDays(int daysUntilLocked, int[] notificationDays)
        {
            // Arrange
            _dateProvider.UserInactivityNotificationDays().Returns(notificationDays);

            // Act
            var result = await _sut.CheckIfUserToBeNotified(10, daysUntilLocked, 999, "test@example.com");

            // Assert
            result.Should().BeOfType<EmailRequestMessage>();
        }

        [Test]
        [TestCase(10, new[] { 10, 2 })]
        [TestCase(1, new[] { 1, 0 })]
        [TestCase(0, new[] { 0, 0, 100 })]
        public async Task CheckIfUserToBeNotifiedInDeletedDays(int daysUntilDeleted, int[] notificationDays)
        {
            // Arrange
            _dateProvider.UserInactivityNotificationDays().Returns(notificationDays);

            // Act
            var result = await _sut.CheckIfUserToBeNotified(10, 999, daysUntilDeleted, "test@example.com");

            // Assert
            result.Should().BeOfType<EmailRequestMessage>();
        }

        [Test]
        [TestCase(10, 30, new[] { 5, 2 })]
        [TestCase(5, 100, new[] { 200, 7 })]
        [TestCase(0, 2, new[] { 5, 7, 100 })]
        public async Task CheckIfUserToBeNotifiedNotInNotificationDays(int daysUntilLocked, int daysUntilDeleted,
            int[] notificationDays)
        {
            // Arrange
            _dateProvider.UserInactivityNotificationDays().Returns(notificationDays);

            // Act
            var result = await _sut.CheckIfUserToBeNotified(10, daysUntilLocked, daysUntilDeleted, "test@example.com");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetLockedEmailRequestMessageReturnsCorrectMessage()
        {
            // Act
            var result = _sut.GetEmailRequestMessage(20, 10, "user_lock", "test@example.com");

            // Assert
            result.Body.Should()
                .Contain("If you do not login to your account in the next 10 day(s), your account will be locked.");
            result.To.Should().Contain("test@example.com");
        }

        [Test]
        public async Task GetDeletedEmailRequestMessageReturnsCorrectMessage()
        {
            // Act
            var result = _sut.GetEmailRequestMessage(20, 10, "user_deletion", "test@example.com");

            // Assert
            result.Body.Should()
                .Contain("If you do not login to your account in the next 10 day(s), your account will be deleted.");
            result.To.Should().Contain("test@example.com");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}