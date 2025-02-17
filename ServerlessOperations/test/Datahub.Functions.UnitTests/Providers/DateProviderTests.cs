using Datahub.Functions.Providers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Datahub.Functions.UnitTests.Providers
{
    [TestFixture]
    public class DateProviderTests
    {
        private DateProvider _dateProvider;
        private IConfiguration _config;
        private AzureConfig _azureConfig;

        [SetUp]
        public void SetUp()
        {
            var jsonConfig = @"
            {
                ""ProjectInactivityNotificationDays"": ""7,2"",
                ""ProjectInactivityDeletionDays"": ""30"",
                ""UserInactivityNotificationDays"":  ""7,2"",
                ""UserInactivityLockedDays"":  ""30"",
                ""UserInactivityDeletionDays"":  ""60""
            }";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonStream(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonConfig)));
            _config = configurationBuilder.Build();
            _azureConfig = new AzureConfig(_config);
            _dateProvider = new DateProvider(_azureConfig);
        }

        [Test]
        public void ProjectNotificationDays_ShouldReturnParsedDays()
        {
            // Act
            var result = _dateProvider.ProjectNotificationDays();

            // Assert
            result.Should().BeEquivalentTo(new[] { 7, 2 });
        }

        [Test]
        public void ProjectDeletionDay_ShouldReturnParsedDay()
        {
            // Act
            var result = _dateProvider.ProjectDeletionDay();

            // Assert
            result.Should().Be(30);
        }

        [Test]
        public void UserInactivityNotificationDays_ShouldReturnParsedDays()
        {
            // Act
            var result = _dateProvider.UserInactivityNotificationDays();

            // Assert
            result.Should().BeEquivalentTo(new[] { 7, 2 });
        }

        [Test]
        public void UserInactivityLockedDay_ShouldReturnParsedDay()
        {
            // Act
            var result = _dateProvider.UserInactivityLockedDay();

            // Assert
            result.Should().Be(30);
        }

        [Test]
        public void UserInactivityDeletionDay_ShouldReturnParsedDay()
        {
            // Act
            var result = _dateProvider.UserInactivityDeletionDay();

            // Assert
            result.Should().Be(60);
        }
    }
}
