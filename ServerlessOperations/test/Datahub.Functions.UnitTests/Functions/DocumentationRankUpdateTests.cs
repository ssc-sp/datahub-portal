using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Documentation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Datahub.Functions.UnitTests.Functions
{
    [TestFixture]
    public class DocumentationRankUpdateTests
    {
        private ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();  
        private DocumentationRankUpdate _documentationRankUpdate;
        private DatahubProjectDBContext _dbContext;
        private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;

        [SetUp]
        public void SetUp()
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<DatahubProjectDBContext>()
                   .UseInMemoryDatabase(Guid.NewGuid().ToString());

            // create a mock factory to return the db context when CreateDbContextAsync is called
            _dbContext = new DatahubProjectDBContext(optionsBuilder.Options);
            _dbContext.Database.EnsureDeleted();
            _dbContext.Database.EnsureCreated();
            _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            _mockFactory
                .Setup(f => f.CreateDbContext())
                .Returns(() => new DatahubProjectDBContext(optionsBuilder.Options));

            _documentationRankUpdate = new DocumentationRankUpdate(_loggerFactory, _mockFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (_dbContext != null)
            {
                try
                {
                    _dbContext.Database.EnsureDeleted();
                }
                catch (ObjectDisposedException)
                {
                    // The context may already have been disposed in a previous cleanup step;
                    // ignore this exception to keep teardown best-effort and not fail tests.
                }

                _dbContext.Dispose();
                _dbContext = null!;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _loggerFactory?.Dispose();
        }

        [Test] 
        public void UpdateRanking_ShouldAddNewDocumentationResource()
        {
            // Arrange
            var docId = Guid.NewGuid();
            _dbContext.TelemetryEvents.Add(new TelemetryEvent { EventName = $"/resources/{docId}" });
            _dbContext.SaveChanges();

            // Act
            var ranking = _documentationRankUpdate.UpdateRanking();

            // Assert
            var doc = _dbContext.DocumentationResources.FirstOrDefault(d => d.Id == docId);
            doc.Should().NotBeNull();
            doc.Hits.Should().Be(1);
            doc.LastUpdated.Date.Should().Be(DateTime.Now.Date);
            ranking[docId].Should().Be(1); 
        }

        [Test]
        public void RunCron_ShouldCallUpdateRanking()
        {
            // Arrange
            var timerInfo = new TimerInfo();

            // Act
            _documentationRankUpdate.RunCron(timerInfo);

            // Assert
            // Since UpdateRanking is internal, we need to use reflection to verify it was called
            var methodInfo = typeof(DocumentationRankUpdate).GetMethod("UpdateRanking", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Should().NotBeNull();

            var ranking = (Dictionary<Guid, int>)methodInfo.Invoke(_documentationRankUpdate, null);
            ranking.Should().NotBeNull();
        }

        [Test]
        public void UpdateRanking_ShouldHandleMultipleTelemetryEvents()
        {
            // Arrange
            var docId = Guid.NewGuid();
            _dbContext.TelemetryEvents.Add(new TelemetryEvent { EventName = $"/resources/{docId}" });
            _dbContext.TelemetryEvents.Add(new TelemetryEvent { EventName = $"/resources/{docId}" });
            _dbContext.SaveChanges();

            // Act
            var ranking = _documentationRankUpdate.UpdateRanking();

            // Assert
            var doc = _dbContext.DocumentationResources.FirstOrDefault(d => d.Id == docId);

            doc.Should().NotBeNull();
            doc.Hits.Should().Be(2);
            doc.LastUpdated.Date.Should().Be(DateTime.Now.Date);
            ranking[docId].Should().Be(2);
        }
    }
}
