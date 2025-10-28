using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Publishing;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services;

using static Testing;

public class OpenGovBlocklistServiceTests
{
    private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;
    private Mock<IUserInformationService> _mockUserInformationService = null!;
    private DatahubProjectDBContext _dbContext = null!;
    private PortalUser _testCurrentUser = null!;

    [SetUp]
    public void Setup()
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SqlServerDatahubContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()); // Use unique DB name for each test
        
        _dbContext = new SqlServerDatahubContext(optionsBuilder.Options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        // Create test user
        _testCurrentUser = new PortalUser
        {
            Id = 1,
            GraphGuid = TestUserGraphGuid,
            Email = TestUserEmail,
            DisplayName = "Test User"
        };
        _dbContext.PortalUsers.Add(_testCurrentUser);
        _dbContext.SaveChanges();

        // Create a mock factory to return the db context when CreateDbContextAsync is called
        _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new SqlServerDatahubContext(optionsBuilder.Options));

        // Create a mock user information service to return the current user
        _mockUserInformationService = new Mock<IUserInformationService>();
        _mockUserInformationService
            .Setup(f => f.GetCurrentPortalUserAsync())
            .ReturnsAsync(_testCurrentUser);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    #region GetActiveBlocklistEntriesAsync Tests

    [Test]
    public async Task GetActiveBlocklistEntriesAsync_ShouldReturnOnlyActiveEntries()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.GetActiveBlocklistEntriesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(e => e.Status == BlocklistStatus.Active), Is.True);
        });
    }

    [Test]
    public async Task GetActiveBlocklistEntriesAsync_ShouldOrderByDateAddedDescending()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.GetActiveBlocklistEntriesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].DateAdded, Is.GreaterThan(result[1].DateAdded));
        });
    }

    [Test]
    public async Task GetActiveBlocklistEntriesAsync_ShouldIncludeAddedByUser()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.GetActiveBlocklistEntriesAsync();

        // Assert
        Assert.That(result.All(e => e.AddedByUser != null), Is.True);
    }

    [Test]
    public async Task GetActiveBlocklistEntriesAsync_ShouldReturnEmptyListWhenNoActiveEntries()
    {
        // Arrange
        var service = GetBlocklistService();

        // Act
        var result = await service.GetActiveBlocklistEntriesAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    #endregion

    #region GetBlocklistEntryAsync Tests

    [Test]
    public async Task GetBlocklistEntryAsync_ShouldReturnEntryWithUserDetails()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();

        // Act
        var result = await service.GetBlocklistEntryAsync(existingEntry.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(existingEntry.Id));
            Assert.That(result.AddedByUser, Is.Not.Null);
        });
    }

    [Test]
    public void GetBlocklistEntryAsync_ShouldThrowWhenEntryNotFound()
    {
        // Arrange
        var service = GetBlocklistService();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GetBlocklistEntryAsync(999));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    #endregion

    #region IsUserBlockedAsync Tests

    [Test]
    public async Task IsUserBlockedAsync_ShouldReturnTrueWhenEmailDomainIsBlocked()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.IsUserBlockedAsync("@dfo-mpo.gc.ca");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsUserBlockedAsync_ShouldReturnFalseWhenEmailDomainNotBlocked()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.IsUserBlockedAsync("@notblocked.gc.ca");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserBlockedAsync_ShouldReturnFalseWhenEmailDomainIsNull()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.IsUserBlockedAsync(null);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserBlockedAsync_ShouldReturnFalseWhenEmailDomainIsWhitespace()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();

        // Act
        var result = await service.IsUserBlockedAsync("   ");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUserBlockedAsync_ShouldReturnFalseWhenEntryIsDeleted()
    {
        // Arrange
        var service = GetBlocklistService();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        
        ctx.OpenGovPublishingBlocklist.Add(new OpenGovPublishingBlocklist
        {
            DepartmentName = "Deleted Department",
            EmailHostname = "@deleted.gc.ca",
            Status = BlocklistStatus.Deleted,
            DateAdded = DateTime.UtcNow,
            AddedByUserId = _testCurrentUser.Id
        });
        await ctx.SaveChangesAsync();

        // Act
        var result = await service.IsUserBlockedAsync("@deleted.gc.ca");

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region AddBlocklistEntryAsync Tests

    [Test]
    public async Task AddBlocklistEntryAsync_ShouldCreateNewEntry()
    {
        // Arrange
        var service = GetBlocklistService();
        var departmentName = "Test Department";
        var emailHostname = "@test.gc.ca";
        var notes = "Test notes";

        // Act
        var result = await service.AddBlocklistEntryAsync(departmentName, emailHostname, notes);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DepartmentName, Is.EqualTo(departmentName));
            Assert.That(result.EmailHostname, Is.EqualTo(emailHostname.ToLowerInvariant()));
            Assert.That(result.Notes, Is.EqualTo(notes));
            Assert.That(result.Status, Is.EqualTo(BlocklistStatus.Active));
            Assert.That(result.AddedByUserId, Is.EqualTo(_testCurrentUser.Id));
            Assert.That(result.DateAdded, Is.Not.EqualTo(default(DateTime)));
        });
    }

    [Test]
    public async Task AddBlocklistEntryAsync_ShouldTrimAndLowercaseEmailHostname()
    {
        // Arrange
        var service = GetBlocklistService();
        var emailHostname = "  @TEST.GC.CA  ";

        // Act
        var result = await service.AddBlocklistEntryAsync("Test Dept", emailHostname, "");

        // Assert
        Assert.That(result.EmailHostname, Is.EqualTo("@test.gc.ca"));
    }

    [Test]
    public async Task AddBlocklistEntryAsync_ShouldTrimDepartmentName()
    {
        // Arrange
        var service = GetBlocklistService();
        var departmentName = "  Test Department  ";

        // Act
        var result = await service.AddBlocklistEntryAsync(departmentName, "@test.gc.ca", "");

        // Assert
        Assert.That(result.DepartmentName, Is.EqualTo("Test Department"));
    }

    [Test]
    public async Task AddBlocklistEntryAsync_ShouldHandleNullNotes()
    {
        // Arrange
        var service = GetBlocklistService();

        // Act
        var result = await service.AddBlocklistEntryAsync("Test Dept", "@test.gc.ca", null);

        // Assert
        Assert.That(result.Notes, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task AddBlocklistEntryAsync_ShouldHandleEmptyDepartmentName()
    {
        // Arrange
        var service = GetBlocklistService();

        // Act
        var result = await service.AddBlocklistEntryAsync("", "@test.gc.ca", "");

        // Assert
        Assert.That(result.DepartmentName, Is.EqualTo(string.Empty));
    }

    [Test]
    public void AddBlocklistEntryAsync_ShouldThrowWhenCurrentUserNotFound()
    {
        // Arrange
        _mockUserInformationService
            .Setup(f => f.GetCurrentPortalUserAsync())
            .ReturnsAsync((PortalUser)null);
        var service = GetBlocklistService();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.AddBlocklistEntryAsync("Test", "@test.gc.ca", ""));
        Assert.That(ex.Message, Does.Contain("Current user not found"));
    }

    [Test]
    public async Task AddBlocklistEntryAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var service = GetBlocklistService();
        var departmentName = "Test Department";
        var emailHostname = "@test.gc.ca";

        // Act
        var result = await service.AddBlocklistEntryAsync(departmentName, emailHostname, "Test");

        // Assert - Verify it's in the database
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var dbEntry = await ctx.OpenGovPublishingBlocklist.FindAsync(result.Id);
        Assert.Multiple(() =>
        {
            Assert.That(dbEntry, Is.Not.Null);
            Assert.That(dbEntry.DepartmentName, Is.EqualTo(departmentName));
            Assert.That(dbEntry.EmailHostname, Is.EqualTo(emailHostname));
        });
    }

    #endregion

    #region UpdateBlocklistEntryAsync Tests

    [Test]
    public async Task UpdateBlocklistEntryAsync_ShouldUpdateExistingEntry()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();
        var newDepartmentName = "Updated Department";
        var newEmailHostname = "@updated.gc.ca";
        var newNotes = "Updated notes";

        // Act
        var result = await service.UpdateBlocklistEntryAsync(
            existingEntry.Id, 
            newDepartmentName, 
            newEmailHostname, 
            newNotes);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.DepartmentName, Is.EqualTo(newDepartmentName));
            Assert.That(result.EmailHostname, Is.EqualTo(newEmailHostname.ToLowerInvariant()));
            Assert.That(result.Notes, Is.EqualTo(newNotes));
        });
    }

    [Test]
    public async Task UpdateBlocklistEntryAsync_ShouldTrimAndLowercaseValues()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();

        // Act
        var result = await service.UpdateBlocklistEntryAsync(
            existingEntry.Id,
            "  Updated Dept  ",
            "  @UPDATED.GC.CA  ",
            "Notes");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.DepartmentName, Is.EqualTo("Updated Dept"));
            Assert.That(result.EmailHostname, Is.EqualTo("@updated.gc.ca"));
        });
    }

    [Test]
    public async Task UpdateBlocklistEntryAsync_ShouldSetNullForWhitespaceDepartmentName()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();

        // Act
        var result = await service.UpdateBlocklistEntryAsync(
            existingEntry.Id,
            "   ",
            "@test.gc.ca",
            "Notes");

        // Assert
        Assert.That(result.DepartmentName, Is.Null);
    }

    [Test]
    public async Task UpdateBlocklistEntryAsync_ShouldSetNullForWhitespaceEmailHostname()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();

        // Act
        var result = await service.UpdateBlocklistEntryAsync(
            existingEntry.Id,
            "Test Dept",
            "   ",
            "Notes");

        // Assert
        Assert.That(result.EmailHostname, Is.Null);
    }

    [Test]
    public void UpdateBlocklistEntryAsync_ShouldThrowWhenEntryNotFound()
    {
        // Arrange
        var service = GetBlocklistService();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.UpdateBlocklistEntryAsync(999, "Test", "@test.gc.ca", ""));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateBlocklistEntryAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();
        var newDepartmentName = "Persisted Update";

        // Act
        await service.UpdateBlocklistEntryAsync(
            existingEntry.Id,
            newDepartmentName,
            "@persist.gc.ca",
            "Notes");

        // Assert - Verify changes are in database
        await using var verifyCtx = await _mockFactory.Object.CreateDbContextAsync();
        var dbEntry = await verifyCtx.OpenGovPublishingBlocklist.FindAsync(existingEntry.Id);
        Assert.That(dbEntry.DepartmentName, Is.EqualTo(newDepartmentName));
    }

    #endregion

    #region DeleteBlocklistEntryAsync Tests

    [Test]
    public async Task DeleteBlocklistEntryAsync_ShouldMarkEntryAsDeleted()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();

        // Act
        await service.DeleteBlocklistEntryAsync(existingEntry.Id);

        // Assert
        await using var verifyCtx = await _mockFactory.Object.CreateDbContextAsync();
        var dbEntry = await verifyCtx.OpenGovPublishingBlocklist.FindAsync(existingEntry.Id);
        Assert.Multiple(() =>
        {
            Assert.That(dbEntry.Status, Is.EqualTo(BlocklistStatus.Deleted));
            Assert.That(dbEntry.DateRemoved, Is.Not.Null);
            Assert.That(dbEntry.RemovedByUserId, Is.EqualTo(_testCurrentUser.Id));
        });
    }

    [Test]
    public async Task DeleteBlocklistEntryAsync_ShouldSetDateRemoved()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();
        var beforeDelete = DateTime.UtcNow;

        // Act
        await service.DeleteBlocklistEntryAsync(existingEntry.Id);

        // Assert
        await using var verifyCtx = await _mockFactory.Object.CreateDbContextAsync();
        var dbEntry = await verifyCtx.OpenGovPublishingBlocklist.FindAsync(existingEntry.Id);
        Assert.Multiple(() =>
        {
            Assert.That(dbEntry.DateRemoved, Is.Not.Null);
            Assert.That(dbEntry.DateRemoved.Value, Is.GreaterThanOrEqualTo(beforeDelete));
            Assert.That(dbEntry.DateRemoved.Value, Is.LessThanOrEqualTo(DateTime.UtcNow));
        });
    }

    [Test]
    public void DeleteBlocklistEntryAsync_ShouldThrowWhenEntryNotFound()
    {
        // Arrange
        var service = GetBlocklistService();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.DeleteBlocklistEntryAsync(999));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task DeleteBlocklistEntryAsync_ShouldThrowWhenCurrentUserNotFound()
    {
        // Arrange
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();
        
        _mockUserInformationService
            .Setup(f => f.GetCurrentPortalUserAsync())
            .ReturnsAsync((PortalUser)null);
        var service = GetBlocklistService();

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.DeleteBlocklistEntryAsync(existingEntry.Id));
        Assert.That(ex.Message, Does.Contain("Current user not found"));
    }

    [Test]
    public async Task DeleteBlocklistEntryAsync_ShouldNotRemoveFromDatabase()
    {
        // Arrange
        var service = GetBlocklistService();
        await SeedBlocklistData();
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();
        var existingEntry = await ctx.OpenGovPublishingBlocklist.FirstAsync();
        var entryId = existingEntry.Id;

        // Act
        await service.DeleteBlocklistEntryAsync(entryId);

        // Assert - Entry should still exist in database, just marked as deleted
        await using var verifyCtx = await _mockFactory.Object.CreateDbContextAsync();
        var dbEntry = await verifyCtx.OpenGovPublishingBlocklist.FindAsync(entryId);
        Assert.That(dbEntry, Is.Not.Null);
    }

    #endregion

    #region Helper Methods

    private OpenGovBlocklistService GetBlocklistService()
    {
        return new OpenGovBlocklistService(
            _mockFactory.Object,
            _mockUserInformationService.Object);
    }

    private async Task SeedBlocklistData()
    {
        await using var ctx = await _mockFactory.Object.CreateDbContextAsync();

        var entries = new List<OpenGovPublishingBlocklist>
        {
            new()
            {
                DepartmentName = "Fisheries and Oceans Canada",
                EmailHostname = "@dfo-mpo.gc.ca",
                Status = BlocklistStatus.Active,
                DateAdded = DateTime.UtcNow.AddDays(-2),
                AddedByUserId = _testCurrentUser.Id,
                Notes = "Test entry 1"
            },
            new()
            {
                DepartmentName = "Environment and Climate Change Canada",
                EmailHostname = "@ec.gc.ca",
                Status = BlocklistStatus.Active,
                DateAdded = DateTime.UtcNow.AddDays(-1),
                AddedByUserId = _testCurrentUser.Id,
                Notes = "Test entry 2"
            },
            new()
            {
                DepartmentName = "Deleted Department",
                EmailHostname = "@deleted.gc.ca",
                Status = BlocklistStatus.Deleted,
                DateAdded = DateTime.UtcNow.AddDays(-3),
                DateRemoved = DateTime.UtcNow.AddDays(-1),
                AddedByUserId = _testCurrentUser.Id,
                RemovedByUserId = _testCurrentUser.Id,
                Notes = "This entry was deleted"
            }
        };

        await ctx.OpenGovPublishingBlocklist.AddRangeAsync(entries);
        await ctx.SaveChangesAsync();
    }

    #endregion
}
