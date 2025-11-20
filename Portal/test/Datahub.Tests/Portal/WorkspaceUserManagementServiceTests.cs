using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Achievements;
using Moq;
using Xunit;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Datahub.Core.Model.Context;
using Xunit.Abstractions;
using Datahub.Core.Data;
using Datahub.Core.Services.Projects;
using System.Linq;
using System.Threading;
using Datahub.Application.Commands;
using Microsoft.Extensions.Logging;
using Datahub.Core.Model.Users;

[assembly: CaptureConsole]

namespace Datahub.Tests
{
    public class WorkspaceUserManagementServiceTests
    {
        private readonly IProjectUserManagementService _projectUserManagementService;
        private readonly Mock<IDbContextFactory<DatahubProjectDBContext>> _mockDbContextFactory;
        private readonly Mock<IUserInformationService> _userInformationService;

        private static readonly string TEST_WORKSPACE_CODE = "TEST";
        
        public WorkspaceUserManagementServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            _mockDbContextFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            _mockDbContextFactory.Setup(f => f.CreateDbContext())
                .Returns(() => new DatahubProjectDBContext(new DbContextOptionsBuilder<DatahubProjectDBContext>().UseInMemoryDatabase(dbName).Options));
            _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(new DatahubProjectDBContext(new DbContextOptionsBuilder<DatahubProjectDBContext>().UseInMemoryDatabase(dbName).Options)));
            
            
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<ProjectUserManagementService>();

            var msGraphService = new Mock<IMSGraphService>();
            var requestManagementService = new Mock<IRequestManagementService>();
            var resourceMessagingService = new Mock<IResourceMessagingService>();
            var userEnrollmentService = new Mock<IUserEnrollmentService>();
            var dhAuditService = new Mock<IDatahubAuditingService>();

            _userInformationService = new Mock<IUserInformationService>();

            SetupDbAndUsers();

            _projectUserManagementService = new ProjectUserManagementService(logger, _mockDbContextFactory.Object,
                _userInformationService.Object, msGraphService.Object, requestManagementService.Object, resourceMessagingService.Object,
                userEnrollmentService.Object, dhAuditService.Object);
            
        }

        private void SetupDbAndUsers()
        {
            var mockUsers = SetupProjectUsers();
            var ctx = _mockDbContextFactory.Object.CreateDbContext();
            ctx.UserRolesLinks.AddRange(mockUsers);
            ctx.SaveChanges();

            _userInformationService.Setup(u => u.GetCurrentPortalUserAsync())
                .Returns(Task.FromResult(mockUsers.First().PortalUser));
        }

        private static IEnumerable<UserRoleLinks> SetupProjectUsers()
        {
            var workspaceLeadRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.WorkspaceLead,
                Name = RoleConstants.WORKSPACE_LEAD_ROLE,
                Description = RoleConstants.WORKSPACE_LEAD_ROLE
            };
            var adminRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.Admin,
                Name = RoleConstants.ADMIN_ROLE,
                Description = RoleConstants.ADMIN_ROLE
            };
            var guestRole = new Project_Role()
            {
                Id = (int)Project_Role.RoleNames.Guest,
                Name = RoleConstants.GUEST_ROLE,
                Description = RoleConstants.GUEST_ROLE
            };

            var workspace = new Datahub_Project() { Project_ID = 1, Project_Acronym_CD = TEST_WORKSPACE_CODE };

            yield return new UserRoleLinks()
            {
                PortalUserId = 1,
                PortalUser = new PortalUser() { Id = 1, GraphGuid = Guid.NewGuid().ToString(), DisplayName = "Walter Lead", Email = "wlead@example.com" },
                Role = workspaceLeadRole,
                RoleId = workspaceLeadRole.Id,
                Project = workspace,
                Project_ID = workspace.Project_ID,
                IsDataSteward = true
            };

            yield return new UserRoleLinks()
            {
                PortalUserId = 2,
                PortalUser = new PortalUser() { Id = 2, GraphGuid = Guid.NewGuid().ToString(), DisplayName = "Nathan Admi", Email = "admin@example.com" },
                Project = workspace,
                Project_ID = workspace.Project_ID,
                Role = adminRole,
                RoleId = adminRole.Id
            };

            yield return new UserRoleLinks()
            {
                PortalUserId = 3,
                PortalUser = new PortalUser() { Id = 3, GraphGuid = Guid.NewGuid().ToString(), DisplayName = "Gary Guest", Email = "guest@example.com" },
                Project = workspace,
                Project_ID = workspace.Project_ID,
                Role = guestRole,
                RoleId = guestRole.Id
            };
        }

        [Fact]
        public async Task TestWorkspaceHasUsers()
        {
            var users = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(users);
            Assert.Equal(3, users.Count);
        }

        [Fact]
        public async Task TestSetDataStewardForAdminUser_ShouldSuccessfullySetDataSteward()
        {
            var usersBefore = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersBefore);

            var userBefore = usersBefore.FirstOrDefault(u => u.RoleId == (int)Project_Role.RoleNames.Admin);
            Assert.NotNull(userBefore);
            Assert.NotNull(userBefore.RoleId);
            Assert.False(userBefore.IsDataSteward);

            var updateCommand = new ProjectUserUpdateCommand()
            {
                IsDataSteward = true,
                NewRoleId = userBefore.RoleId.Value,
                ProjectUser = userBefore
            };

            await _projectUserManagementService.ProcessProjectUserCommandsAsync([updateCommand], [], "1");

            var usersAfter = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersAfter);

            var userAfter = usersAfter.FirstOrDefault(u => u.PortalUserId == userBefore.PortalUserId);
            Assert.NotNull(userAfter);
            Assert.True(userAfter.IsDataSteward);
        }

        [Fact]
        public async Task TestSetDataStewardOnGuest_ShouldFailWithDataStewardStillUnset()
        {
            var usersBefore = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersBefore);

            var userBefore = usersBefore.FirstOrDefault(u => u.RoleId == (int)Project_Role.RoleNames.Guest);
            Assert.NotNull(userBefore);
            Assert.NotNull(userBefore.RoleId);
            Assert.False(userBefore.IsDataSteward);

            var updateCommand = new ProjectUserUpdateCommand()
            {
                IsDataSteward = true,
                NewRoleId = userBefore.RoleId.Value,
                ProjectUser = userBefore
            };

            await _projectUserManagementService.ProcessProjectUserCommandsAsync([updateCommand], [], "1");

            var usersAfter = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersAfter);

            var userAfter = usersAfter.FirstOrDefault(u => u.PortalUserId == userBefore.PortalUserId);
            Assert.NotNull(userAfter);
            Assert.False(userAfter.IsDataSteward);
        }

        [Fact]
        public async Task TestChangeWorkspaceLeadToGuest_ShouldUnsetDataSteward()
        {
            var usersBefore = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersBefore);

            var userBefore = usersBefore.FirstOrDefault(u => u.RoleId == (int)Project_Role.RoleNames.WorkspaceLead);
            Assert.NotNull(userBefore);
            Assert.NotNull(userBefore.RoleId);
            Assert.True(userBefore.IsDataSteward);

            var updateCommand = new ProjectUserUpdateCommand()
            {
                IsDataSteward = userBefore.IsDataSteward,
                NewRoleId = (int)Project_Role.RoleNames.Guest,
                ProjectUser = userBefore
            };

            await _projectUserManagementService.ProcessProjectUserCommandsAsync([updateCommand], [], "1");

            var usersAfter = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersAfter);

            var userAfter = usersAfter.FirstOrDefault(u => u.PortalUserId == userBefore.PortalUserId);
            Assert.NotNull(userAfter);
            Assert.Equal((int)Project_Role.RoleNames.Guest, userAfter.RoleId);
            Assert.False(userAfter.IsDataSteward);
        }

        [Fact]
        public async Task TestChangeGuestToCollabWithDataSteward_ShouldHaveNewRoleAndDataSteward()
        {
            var usersBefore = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersBefore);

            var userBefore = usersBefore.FirstOrDefault(u => u.RoleId == (int)Project_Role.RoleNames.Guest);
            Assert.NotNull(userBefore);
            Assert.NotNull(userBefore.RoleId);
            Assert.False(userBefore.IsDataSteward);

            var updateCommand = new ProjectUserUpdateCommand()
            {
                IsDataSteward = true,
                NewRoleId = (int)Project_Role.RoleNames.Collaborator,
                ProjectUser = userBefore
            };

            await _projectUserManagementService.ProcessProjectUserCommandsAsync([updateCommand], [], "1");

            var usersAfter = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersAfter);

            var userAfter = usersAfter.FirstOrDefault(u => u.PortalUserId == userBefore.PortalUserId);
            Assert.NotNull(userAfter);
            Assert.Equal((int)Project_Role.RoleNames.Collaborator, userAfter.RoleId);
            Assert.True(userAfter.IsDataSteward);
        }

        [Fact]
        public async Task TestChangeRoleWithoutSettingDataSteward_ShouldHaveRoleButNotDataSteward()
        {
            var usersBefore = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersBefore);

            var userBefore = usersBefore.FirstOrDefault(u => u.RoleId == (int)Project_Role.RoleNames.Admin);
            Assert.NotNull(userBefore);
            Assert.NotNull(userBefore.RoleId);
            Assert.False(userBefore.IsDataSteward);

            var updateCommand = new ProjectUserUpdateCommand()
            {
                IsDataSteward = userBefore.IsDataSteward,
                NewRoleId = (int)Project_Role.RoleNames.Collaborator,
                ProjectUser = userBefore
            };

            await _projectUserManagementService.ProcessProjectUserCommandsAsync([updateCommand], [], "1");

            var usersAfter = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersAfter);

            var userAfter = usersAfter.FirstOrDefault(u => u.PortalUserId == userBefore.PortalUserId);
            Assert.NotNull(userAfter);
            Assert.Equal((int)Project_Role.RoleNames.Collaborator, userAfter.RoleId);
            Assert.False(userAfter.IsDataSteward);
        }

        [Fact]
        public async Task TestUserRoleChange_ShouldAddPortalUserRoleChangeToDbContext()
        {
            // Arrange
            var usersBefore = await _projectUserManagementService.GetProjectUsersAsync(TEST_WORKSPACE_CODE);
            Assert.NotNull(usersBefore);

            var userBefore = usersBefore.FirstOrDefault(u => u.RoleId == (int)Project_Role.RoleNames.Admin);
            Assert.NotNull(userBefore);
            Assert.NotNull(userBefore.RoleId);

            var updateCommand = new ProjectUserUpdateCommand()
            {
                IsDataSteward = userBefore.IsDataSteward,
                NewRoleId = (int)Project_Role.RoleNames.Collaborator, // Change role
                ProjectUser = userBefore
            };

            var ctx = _mockDbContextFactory.Object.CreateDbContext();

            // Act
            await _projectUserManagementService.ProcessProjectUserCommandsAsync(
                new List<ProjectUserUpdateCommand> { updateCommand },
                new List<ProjectUserAddEntraUserCommand>(),
                "1"
            );

            // Assert
            var roleChangeRecord = await ctx.PortalUserRoleChanges.FirstOrDefaultAsync(r =>
                r.PortalUserId == userBefore.PortalUserId && 
                r.RoleId == Project_Role.RoleNames.Collaborator &&
                r.ChangeDate != default
            );

            Assert.NotNull(roleChangeRecord);
            Assert.Equal(userBefore.PortalUserId, roleChangeRecord.PortalUserId); 
            Assert.Equal(updateCommand.NewRoleId, (int)roleChangeRecord.RoleId);
            Assert.NotEqual(default, roleChangeRecord.ChangeDate);
        }

    }
}