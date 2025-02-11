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
                   .UseInMemoryDatabase(new Guid().ToString());

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
        [OneTimeTearDown]
        public void TearDown()
        {
            if (_dbContext!=null)
            {
                _dbContext.Database.EnsureDeleted();
                _dbContext.Dispose();
            }
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
