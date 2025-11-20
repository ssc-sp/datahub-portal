using System.Linq;
using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Datahub.Application.Services;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Services.Storage;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services;

/// <summary>
/// Test-friendly version of WorkspaceAclService that allows mocking Azure Storage clients
/// </summary>
public class TestableWorkspaceAclService : WorkspaceAclService
{
    private readonly DataLakeServiceClient? _mockServiceClient;

    public TestableWorkspaceAclService(
        ILogger<WorkspaceAclService> logger,
        IProjectStorageConfigurationService storageConfig,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        DataLakeServiceClient? mockServiceClient = null)
        : base(logger, storageConfig, dbContextFactory)
    {
        _mockServiceClient = mockServiceClient;
    }

    protected override Task<DataLakeServiceClient> GetDataLakeServiceClientAsync(string workspaceAcronym)
    {
        if (_mockServiceClient != null)
        {
            return Task.FromResult(_mockServiceClient);
        }
        return base.GetDataLakeServiceClientAsync(workspaceAcronym);
    }
}

[TestFixture]
public class WorkspaceAclServiceTests
{
    private ILogger<WorkspaceAclService> _logger = null!;
    private IProjectStorageConfigurationService _storageConfig = null!;
    private IDbContextFactory<DatahubProjectDBContext> _dbContextFactory = null!;

    private const string TestWorkspaceAcronym = "TEST";
    private const string TestStorageAccountName = "teststorageaccount";
    private const string TestStorageAccountKey = "dGVzdGtleQ=="; // base64 encoded "testkey"
    private static readonly string[] TestUserIds = { "user1-guid", "user2-guid", "user3-guid" };

    [SetUp]
    public void Setup()
    {
        // Setup mocks
        _logger = Substitute.For<ILogger<WorkspaceAclService>>();
        _storageConfig = Substitute.For<IProjectStorageConfigurationService>();
        _dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();

        _storageConfig.GetProjectStorageAccountName(TestWorkspaceAcronym)
            .Returns(TestStorageAccountName);
        
        _storageConfig.GetProjectStorageAccountKey(TestWorkspaceAcronym)
            .Returns(Task.FromResult(TestStorageAccountKey));
    }

    [Test]
    public async Task GetWorkspaceAsync_ShouldReturnWorkspace_WhenWorkspaceExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Act
        var result = await service.GetWorkspaceAsync(TestWorkspaceAcronym);

        // Assert
        result.Should().NotBeNull();
        result!.Project_Acronym_CD.Should().Be(TestWorkspaceAcronym);
        result.Project_Name.Should().Be("Test Project");
    }

    [Test]
    public async Task GetWorkspaceAsync_ShouldReturnNull_WhenWorkspaceDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Act
        var result = await service.GetWorkspaceAsync("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetWorkspaceMemberIdsAsync_ShouldReturnMemberIds_WhenWorkspaceHasMembers()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var users = TestUserIds.Select((id, index) => new PortalUser
        {
            GraphGuid = id,
            Email = $"user{index + 1}@test.gc.ca",
            DisplayName = $"Test User {index + 1}"
        }).ToList();

        context.PortalUsers.AddRange(users);
        await context.SaveChangesAsync();

        // Create roles linking users to project
        foreach (var user in users)
        {
            context.UserRolesLinks.Add(new UserRoleLinks
            {
                Project = project,
                PortalUser = user,
                Role = new Project_Role { Name = "Collaborator", Description = "Test role" }
            });
        }
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Act
        var result = await service.GetWorkspaceMemberIdsAsync(TestWorkspaceAcronym);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(TestUserIds.Length);
        result.Should().Contain(TestUserIds);
    }

    [Test]
    public async Task GetWorkspaceMemberIdsAsync_ShouldReturnEmptyList_WhenWorkspaceDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Act
        var result = await service.GetWorkspaceMemberIdsAsync("NONEXISTENT");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Constructor_ShouldInitializeWithRequiredDependencies()
    {
        // Act
        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IWorkspaceAclService>();
    }

    [Test]
    public async Task RemoveAllUserAclsFromPathAsync_WithMockedStorage_ShouldUseInjectedClient()
    {
        // This test verifies that TestableWorkspaceAclService correctly uses injected mock client
        // Note: We can't easily mock Azure SDK's AsyncPageable<PathItem> return type,
        // so this test verifies the service gets the mock client and attempts to use it
        
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        // Create mock Azure Storage clients
        var mockServiceClient = Substitute.For<DataLakeServiceClient>();
        var mockFileSystemClient = Substitute.For<DataLakeFileSystemClient>();
        
        // Setup the mock chain - service will call GetFileSystemClient
        mockServiceClient.GetFileSystemClient("datahub").Returns(mockFileSystemClient);

        var service = new TestableWorkspaceAclService(_logger, _storageConfig, _dbContextFactory, mockServiceClient);

        // Act
        var result = await service.RemoveAllUserAclsFromPathAsync(
            TestWorkspaceAcronym, "/upload", recursive: true);

        // Assert  
        result.Should().Be(0); // No files in mock (GetPathsAsync returns empty by default)
        
        // Verify the service did NOT call real storage config (proves mock injection worked)
        _storageConfig.DidNotReceive().GetProjectStorageAccountName(TestWorkspaceAcronym);
        await _storageConfig.DidNotReceive().GetProjectStorageAccountKey(TestWorkspaceAcronym);
        
        // Verify file system client was requested from our injected mock
        mockServiceClient.Received(1).GetFileSystemClient("datahub");
    }

    [Test]
    public async Task SimulateScanSuccessAsync_WithMockedStorage_ShouldUseInjectedClient()
    {
        // This test verifies that TestableWorkspaceAclService correctly uses injected mock client
        
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var users = TestUserIds.Select((id, index) => new PortalUser
        {
            GraphGuid = id,
            Email = $"user{index + 1}@test.gc.ca",
            DisplayName = $"Test User {index + 1}"
        }).ToList();

        context.PortalUsers.AddRange(users);
        await context.SaveChangesAsync();

        foreach (var user in users)
        {
            context.UserRolesLinks.Add(new UserRoleLinks
            {
                Project = project,
                PortalUser = user,
                Role = new Project_Role { Name = "Collaborator", Description = "Test role" }
            });
        }
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        // Create mock Azure Storage clients
        var mockServiceClient = Substitute.For<DataLakeServiceClient>();
        var mockFileSystemClient = Substitute.For<DataLakeFileSystemClient>();
        
        mockServiceClient.GetFileSystemClient("datahub").Returns(mockFileSystemClient);

        var service = new TestableWorkspaceAclService(_logger, _storageConfig, _dbContextFactory, mockServiceClient);

        // Act
        var result = await service.SimulateScanSuccessAsync(TestWorkspaceAcronym, "/upload");

        // Assert
        result.Should().Be(0); // No files in mock (GetPathsAsync returns empty by default)
        
        // Verify storage config was NOT called (proves mock injection worked)
        _storageConfig.DidNotReceive().GetProjectStorageAccountName(TestWorkspaceAcronym);
        await _storageConfig.DidNotReceive().GetProjectStorageAccountKey(TestWorkspaceAcronym);
        
        // Verify file system client was requested from injected mock
        mockServiceClient.Received(1).GetFileSystemClient("datahub");
    }

    [Test]
    public async Task ApplyWorkspaceMemberAclsAsync_WithMockedStorage_ShouldUseInjectedClient()
    {
        // This test verifies that TestableWorkspaceAclService correctly uses injected mock client
        
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var users = TestUserIds.Select((id, index) => new PortalUser
        {
            GraphGuid = id,
            Email = $"user{index + 1}@test.gc.ca",
            DisplayName = $"Test User {index + 1}"
        }).ToList();

        context.PortalUsers.AddRange(users);
        await context.SaveChangesAsync();

        // Create roles linking users to project
        foreach (var user in users)
        {
            context.UserRolesLinks.Add(new UserRoleLinks
            {
                Project = project,
                PortalUser = user,
                Role = new Project_Role { Name = "Collaborator", Description = "Test role" }
            });
        }
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        // Create mock Azure Storage clients
        var mockServiceClient = Substitute.For<DataLakeServiceClient>();
        var mockFileSystemClient = Substitute.For<DataLakeFileSystemClient>();
        
        mockServiceClient.GetFileSystemClient("datahub").Returns(mockFileSystemClient);

        var service = new TestableWorkspaceAclService(_logger, _storageConfig, _dbContextFactory, mockServiceClient);

        // Act & Assert
        // The mock setup is incomplete (GetPropertiesAsync will return null), causing NullReferenceException
        // But we can verify it got far enough to use the injected mock
        Func<Task> act = async () => await service.ApplyWorkspaceMemberAclsAsync(
            TestWorkspaceAcronym, "/test", "r-x", recursive: true);

        await act.Should().ThrowAsync<NullReferenceException>();
        
        // Verify storage config was NOT called (proves mock injection worked)
        _storageConfig.DidNotReceive().GetProjectStorageAccountName(TestWorkspaceAcronym);
        await _storageConfig.DidNotReceive().GetProjectStorageAccountKey(TestWorkspaceAcronym);
        
        // Verify file system client was requested from injected mock
        mockServiceClient.Received(1).GetFileSystemClient("datahub");
        
        // Verify members were retrieved successfully
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Found") && o.ToString()!.Contains("members")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Test]
    public async Task ApplyWorkspaceMemberAclsAsync_ShouldReturnZero_WhenNoMembers()
    {
        // This test verifies early return when workspace has no members
        
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Act
        var result = await service.ApplyWorkspaceMemberAclsAsync(
            TestWorkspaceAcronym, "/test", "r-x", recursive: true);

        // Assert
        result.Should().Be(0);
        
        // Verify warning was logged
        _logger.Received().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("No members found")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
        
        // Storage config should NOT be called if there are no members
        _storageConfig.DidNotReceive().GetProjectStorageAccountName(TestWorkspaceAcronym);
    }

    [Test]
    public async Task GetWorkspaceAsync_ShouldIncludeUserRolesAndPortalUsers()
    {
        // This test verifies navigation properties are loaded correctly
        
        // Arrange
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .Options;
        
        await using var context = new DatahubProjectDBContext(options);
        
        var project = new Datahub_Project
        {
            Project_Acronym_CD = TestWorkspaceAcronym,
            Project_Name = "Test Project",
            Project_Status_Desc = "Active"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        var users = TestUserIds.Select((id, index) => new PortalUser
        {
            GraphGuid = id,
            Email = $"user{index + 1}@test.gc.ca",
            DisplayName = $"Test User {index + 1}"
        }).ToList();

        context.PortalUsers.AddRange(users);
        await context.SaveChangesAsync();

        foreach (var user in users)
        {
            context.UserRolesLinks.Add(new UserRoleLinks
            {
                Project = project,
                PortalUser = user,
                Role = new Project_Role { Name = "Collaborator", Description = "Test role" }
            });
        }
        await context.SaveChangesAsync();

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(context);

        var service = new WorkspaceAclService(_logger, _storageConfig, _dbContextFactory);

        // Act
        var result = await service.GetWorkspaceAsync(TestWorkspaceAcronym);

        // Assert
        result.Should().NotBeNull();
        result!.UserRoles.Should().NotBeEmpty();
        result.UserRoles.Should().HaveCount(TestUserIds.Length);
        result.UserRoles.Should().OnlyContain(ur => ur.PortalUser != null);
        result.UserRoles.Select(ur => ur.PortalUser!.GraphGuid).Should().Contain(TestUserIds);
    }
}
