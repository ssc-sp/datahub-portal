using NSubstitute;
using Azure.Data.Tables;
using Datahub.Functions.Entities;
using Datahub.Functions.Services;
using Datahub.Infrastructure.Queues.Messages;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using System.Linq.Expressions;

namespace Datahub.Functions.UnitTests.Services
{
    [TestFixture]
    public class AlertRecordServiceTests
    {
        private IConfiguration _mockConfig;
        private AzureConfig _azureConfig;
        private AlertRecordService _service;
        private TableClient _alertClient;
        private TableClient _bugReportClient;
        private BugReportMessage _bugReportMessage;
        private IConfiguration _config = Substitute.For<IConfiguration>();

        [SetUp]
        public void Setup()
        {
            var inMemorySettings = new List<KeyValuePair<string, string?>> {
                new KeyValuePair<string, string?>("DatahubStorageConnectionString", "UseDevelopmentStorage=true"),
                new KeyValuePair<string, string?>("InfrastructureAlertDebounceTimeSpan", "00:10:00")
            };

            _mockConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _azureConfig = new AzureConfig(_mockConfig);
            _service = new AlertRecordService(_azureConfig);


            _alertClient = Substitute.For<TableClient>();
            _bugReportClient = Substitute.For<TableClient>();
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
        public async Task GetRecentAlertForBugMessage_ShouldReturnLatestAlert()
        {
            // Arrange
            var reportIdentifier = "test-report-identifier";
            var receivedAlert = new ReceivedAlert { ReportIdentifier = reportIdentifier, EmailSent = true, Timestamp = DateTimeOffset.Now };

            _service = Substitute.ForPartsOf<AlertRecordService>(_azureConfig);
            _service.When(x => x.CreateTableClient(Arg.Any<string>())).DoNotCallBase();
            _service.CreateTableClient(Arg.Any<string>()).Returns(Task.FromResult(_alertClient));
            _alertClient.QueryAsync(Arg.Any<Expression<Func<ReceivedAlert, bool>>>())
                        .Returns(AsyncPageableHelper.CreateAsyncPageable(new[] { receivedAlert }));

            // Act
            var result = await _service.GetRecentAlertForBugMessage(_bugReportMessage);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(receivedAlert);
        }

        [Test]
        public async Task RecordReceivedAlert_ShouldSaveAlertAndBugReport()
        {
            // Arrange
            _service = Substitute.ForPartsOf<AlertRecordService>(_azureConfig);
            _service.When(x => x.CreateTableClient(Arg.Any<string>())).DoNotCallBase();
            _service.CreateTableClient(Arg.Any<string>()).Returns(Task.FromResult(_alertClient));

            // Act
            var result = await _service.RecordReceivedAlert(_bugReportMessage);

            // Assert 
            result.Should().NotBeNull();
            result.PartitionKey.Should().Be("SupportRequest");
            result.ReportIdentifier.Should().Be("Test.DIE1");
        }
    }
}
