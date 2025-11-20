using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using FluentAssertions;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Functions.UnitTests.Functions
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

        private readonly IProjectUserManagementService _projectUserManagementService =
            Substitute.For<IProjectUserManagementService>();

        private readonly IConfiguration _config = Substitute.For<IConfiguration>();


        private AzureConfig _azConfig;
        private IQueuePongService _pongService;
        private EmailValidator _emailValidator;
        private IGCNotifyService _gcNotifyService;
        private ISendEndpointProvider _iSendEndpointProvider;

        [SetUp]
        public async Task Setup()
        {

            _iSendEndpointProvider = Substitute.For<ISendEndpointProvider>();
            _azConfig = new AzureConfig(_config);
            _pongService = new QueuePongService(_iSendEndpointProvider);
            _emailValidator = new EmailValidator();
            _gcNotifyService = Substitute.For<IGCNotifyService>();
            _sut = new UserInactivityNotifier(_loggerFactory, _dbContextFactory, _dateProvider, _azConfig,
                _pongService, _emailValidator, _userInactivityNotificationService, _iSendEndpointProvider,
                _projectUserManagementService, _gcNotifyService);
        }

        [Test]
        [TestCase(10, new[] { 10, 2 })]
        [TestCase(1, new[] { 1, 0 })]
        [TestCase(0, new[] { 0, 0, 100 })]
        public async Task CheckIfUserToBeNotified_InLockedDays(int daysUntilLocked, int[] notificationDays)
        {
            // Arrange
            _dateProvider.UserInactivityNotificationDays().Returns(notificationDays);

            // Act
            var result = await _sut.CheckIfUserToBeNotified(10, daysUntilLocked, 999, "test@example.com");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [TestCase(10, new[] { 10, 2 })]
        [TestCase(1, new[] { 1, 0 })]
        [TestCase(0, new[] { 0, 0, 100 })]
        public async Task CheckIfUserToBeNotified_InDeletedDays(int daysUntilDeleted, int[] notificationDays)
        {
            // Arrange
            _dateProvider.UserInactivityNotificationDays().Returns(notificationDays);

            // Act
            var result = await _sut.CheckIfUserToBeNotified(10, 999, daysUntilDeleted, "test@example.com");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [TestCase(10, 30, new[] { 5, 2 })]
        [TestCase(5, 100, new[] { 200, 7 })]
        [TestCase(0, 2, new[] { 5, 7, 100 })]
        public async Task CheckIfUserToBeNotified_NotInNotificationDays(int daysUntilLocked, int daysUntilDeleted,
            int[] notificationDays)
        {
            // Arrange
            _dateProvider.UserInactivityNotificationDays().Returns(notificationDays);

            // Act
            var result = await _sut.CheckIfUserToBeNotified(10, daysUntilLocked, daysUntilDeleted, "test@example.com");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DisablePortalUser_DisablesUserInAllProjects()
        {
            // Arrange
            var portalUserId = 123;
            var projectId1 = "AAAA";
            var projectId2 = "BBBB";

            // Mock the projects associated with the portal user
            var projects = new List<string>
            {
                projectId1 , projectId2 
            };
            _projectUserManagementService.GetProjectListForPortalUser(portalUserId).Returns(projects);

            // Mock the project users for each project
            var projectUser1 = new UserRoleLinks
            {
                PortalUser = new PortalUser { Id = portalUserId, GraphGuid = Guid.NewGuid().ToString() },
                Role = new Project_Role { Id = (int)Project_Role.RoleNames.WorkspaceLead, Name = "Joe", Description = "Workspace user" }
            };
            var projectUser2 = new UserRoleLinks
            {
                PortalUser = new PortalUser { Id = portalUserId, GraphGuid=Guid.NewGuid().ToString() },
                Role = new Project_Role { Id = (int)Project_Role.RoleNames.Collaborator, Name = "Jane", Description = "Collaborator" }
            };
            _projectUserManagementService.GetProjectUsersAsync(projectId1).Returns(new List<UserRoleLinks> { projectUser1 });
            _projectUserManagementService.GetProjectUsersAsync(projectId2).Returns(new List<UserRoleLinks> { projectUser2 });

            // Act
            await _sut.DisablePortalUser(portalUserId);

            // Assert
            await _projectUserManagementService.Received(1).ProcessProjectUserCommandsAsync(
                Arg.Is<List<ProjectUserUpdateCommand>>(commands =>
                    commands.Count == 2 &&
                    commands.Any(c => c.ProjectUser == projectUser1 && c.NewRoleId == (int)Project_Role.RoleNames.DisabledUser) &&
                    commands.Any(c => c.ProjectUser == projectUser2 && c.NewRoleId == (int)Project_Role.RoleNames.DisabledUser)
                ),
                Arg.Is<List<ProjectUserAddEntraUserCommand>>(addCommands => addCommands.Count == 0),
                portalUserId.ToString()
            );
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _loggerFactory?.Dispose();
        }
    }
}