using Datahub.Application.Services;
using Datahub.Application.Services.Publishing;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.Publishing;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public sealed class PublishingBlocklistSteps
{
    private readonly ScenarioContext _scenarioContext;
    private IDbContextFactory<DatahubProjectDBContext> _mockFactory = null!;
    private IUserInformationService _mockUserInformationService = null!;
    private IMemoryCache _mockMemoryCache = null!;
    private IProjectUserManagementService _mockProjectUserManagementService = null!;
    private IOpenDataPublishingService _publishingService = null!;
    private DatahubProjectDBContext _dbContext = null!;
    private PortalUser _testCurrentUser = null!;
    private OpenGovPublishingBlocklist _currentEntry = null!;
    private List<OpenGovPublishingBlocklist> _activeEntries = null!;
    private bool _isUserBlocked;

    public PublishingBlocklistSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private void SetupService()
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        _dbContext = new SqlServerDatahubContext(optionsBuilder.Options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        // Create test user
        _testCurrentUser = new PortalUser
        {
            Id = 1,
            EntraUser = new EntraUser
            {
                GraphGuid = "test-user-guid",
                PortalUser = null!
            },
            Email = "admin@test.gc.ca",
            DisplayName = "Test Admin User"
        };
        _dbContext.PortalUsers.Add(_testCurrentUser);
        _dbContext.SaveChanges();

        _mockFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .CreateDbContextAsync(CancellationToken.None)
            .Returns(_ => Task.FromResult<DatahubProjectDBContext>(new SqlServerDatahubContext(optionsBuilder.Options)));

        _mockUserInformationService = Substitute.For<IUserInformationService>();
        _mockUserInformationService
            .GetCurrentPortalUserAsync()
            .Returns(Task.FromResult<PortalUser?>(_testCurrentUser));

        _mockMemoryCache = Substitute.For<IMemoryCache>();
        _mockProjectUserManagementService = Substitute.For<IProjectUserManagementService>();

        _publishingService = new OpenDataPublishingService(
            _mockUserInformationService,
            _mockFactory,
            _mockMemoryCache,
            _mockProjectUserManagementService);
    }

    [Given(@"a publishing blocklist service with no existing entries")]
    public void GivenAPublishingBlocklistServiceWithNoExistingEntries()
    {
        SetupService();
    }

    [Given(@"a publishing blocklist service with an entry for email domain ""(.*)""")]
    public async Task GivenAPublishingBlocklistServiceWithAnEntryForEmailDomain(string emailDomain)
    {
        SetupService();
        _currentEntry = await _publishingService.AddBlocklistEntryAsync(
            "Test Department",
            emailDomain,
            "Test notes");
    }

    [Given(@"a publishing blocklist service with (\d+) active entries and (\d+) deleted entry")]
    public async Task GivenAPublishingBlocklistServiceWithActiveAndDeletedEntries(int activeCount, int deletedCount)
    {
        SetupService();

        // Add active entries
        for (int i = 0; i < activeCount; i++)
        {
            await _publishingService.AddBlocklistEntryAsync(
                $"Active Department {i + 1}",
                $"@active{i + 1}.gc.ca",
                $"Active notes {i + 1}");
        }

        // Add and delete entries
        for (int i = 0; i < deletedCount; i++)
        {
            var entry = await _publishingService.AddBlocklistEntryAsync(
                $"Deleted Department {i + 1}",
                $"@deleted{i + 1}.gc.ca",
                $"Deleted notes {i + 1}");
            await _publishingService.DeleteBlocklistEntryAsync(entry.Id);
        }
    }

    [When(@"a new blocklist entry is added with department ""(.*)"" and email domain ""(.*)""")]
    public async Task WhenANewBlocklistEntryIsAddedWithDepartmentAndEmailDomain(string department, string emailDomain)
    {
        _currentEntry = await _publishingService.AddBlocklistEntryAsync(
            department,
            emailDomain,
            "Test notes");
    }

    [When(@"checking if email domain ""(.*)"" is blocked")]
    public async Task WhenCheckingIfEmailDomainIsBlocked(string emailDomain)
    {
        _isUserBlocked = await _publishingService.IsUserBlockedAsync(emailDomain);
    }

    [When(@"the blocklist entry is updated with department ""(.*)"" and email domain ""(.*)""")]
    public async Task WhenTheBlocklistEntryIsUpdatedWithDepartmentAndEmailDomain(string department, string emailDomain)
    {
        _currentEntry = await _publishingService.UpdateBlocklistEntryAsync(
            _currentEntry.Id,
            department,
            emailDomain,
            "Updated notes");
    }

    [When(@"the blocklist entry is deleted")]
    public async Task WhenTheBlocklistEntryIsDeleted()
    {
        await _publishingService.DeleteBlocklistEntryAsync(_currentEntry.Id);
        // Refresh the entry from the database
        await using var ctx = await _mockFactory.CreateDbContextAsync();
        _currentEntry = (await ctx.OpenGovPublishingBlocklist.FindAsync(_currentEntry.Id))!;
    }

    [When(@"retrieving active blocklist entries")]
    public async Task WhenRetrievingActiveBlocklistEntries()
    {
        _activeEntries = await _publishingService.GetActiveBlocklistEntriesAsync();
    }

    [Then(@"the blocklist should contain (\d+) entry")]
    public async Task ThenTheBlocklistShouldContainEntry(int expectedCount)
    {
        await using var ctx = await _mockFactory.CreateDbContextAsync();
        var count = await ctx.OpenGovPublishingBlocklist.CountAsync();
        count.Should().Be(expectedCount);
    }

    [Then(@"the blocklist entry should have department name ""(.*)""")]
    public void ThenTheBlocklistEntryShouldHaveDepartmentName(string expectedDepartment)
    {
        _currentEntry.DepartmentName.Should().Be(expectedDepartment);
    }

    [Then(@"the blocklist entry should have email hostname ""(.*)""")]
    public void ThenTheBlocklistEntryShouldHaveEmailHostname(string expectedEmailHostname)
    {
        _currentEntry.EmailHostname.Should().Be(expectedEmailHostname);
    }

    [Then(@"the blocklist entry status should be Active")]
    public void ThenTheBlocklistEntryStatusShouldBeActive()
    {
        _currentEntry.Status.Should().Be(BlocklistStatus.Active);
    }

    [Then(@"the blocklist entry status should be Deleted")]
    public void ThenTheBlocklistEntryStatusShouldBeDeleted()
    {
        _currentEntry.Status.Should().Be(BlocklistStatus.Deleted);
    }

    [Then(@"the blocklist entry should have a removal date")]
    public void ThenTheBlocklistEntryShouldHaveARemovalDate()
    {
        _currentEntry.DateRemoved.Should().NotBeNull();
        _currentEntry.DateRemoved.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Then(@"the user should be blocked")]
    public void ThenTheUserShouldBeBlocked()
    {
        _isUserBlocked.Should().BeTrue();
    }

    [Then(@"the user should not be blocked")]
    public void ThenTheUserShouldNotBeBlocked()
    {
        _isUserBlocked.Should().BeFalse();
    }

    [Then(@"the user should not be blocked when checking email domain ""(.*)""")]
    public async Task ThenTheUserShouldNotBeBlockedWhenCheckingEmailDomain(string emailDomain)
    {
        var isBlocked = await _publishingService.IsUserBlockedAsync(emailDomain);
        isBlocked.Should().BeFalse();
    }

    [Then(@"the active blocklist entries should contain (\d+) entries")]
    public void ThenTheActiveBlocklistEntriesShouldContainEntries(int expectedCount)
    {
        _activeEntries.Should().HaveCount(expectedCount);
        _activeEntries.Should().OnlyContain(e => e.Status == BlocklistStatus.Active);
    }
}
