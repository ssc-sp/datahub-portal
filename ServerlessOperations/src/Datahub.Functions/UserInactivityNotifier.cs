using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Commands;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Functions.Extensions;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class UserInactivityNotifier(
        ILoggerFactory loggerFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IDateProvider dateProvider,
        AzureConfig config,
        IQueuePongService pongService,
        EmailValidator emailValidator,
        IUserInactivityNotificationService userInactivityNotificationService,
        ISendEndpointProvider sendEndpointProvider,
        IProjectUserManagementService projectUserManagementService,
        IGCNotifyService notifyService)
    {
        private readonly ILogger<UserInactivityNotifier> _logger = loggerFactory.CreateLogger<UserInactivityNotifier>();

        [Function("UserInactivityNotifier")]
        public async Task Run(
            [ServiceBusTrigger(QueueConstants.UserInactivityNotification,
                Connection = "DatahubServiceBus:ConnectionString")]
            ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken ct)
        {
            // test for ping
            // if (await pongService.Pong(serviceBusReceivedMessage.Body.ToString()))
            // return;

            // deserialize message
            var message = await serviceBusReceivedMessage
                .DeserializeAndUnwrapMessageAsync<UserInactivityNotificationMessage>();

            _logger.LogInformation($"Received user notification check for ID: {message.UserId}");

            // verify message 
            if (message is null)
            {
                throw new Exception($"Invalid queue message:\n{serviceBusReceivedMessage.Body}");
            }

            using var ctx = await dbContextFactory.CreateDbContextAsync(ct);
            var user = await ctx.PortalUsers
                .AsNoTracking()
                .Where(x => x.Id == message.UserId)
                .FirstAsync(ct);
            _logger.LogInformation("Found user {UserDisplayName} to check for inactivity notifications",
                user.DisplayName);

            var lastLoginDate = user.LastLoginDateTime ?? user.FirstLoginDateTime;
            int daysSinceLastLogin = (int)(dateProvider.Today - lastLoginDate)?.Days;
            int daysUntilLocked = (int)(dateProvider.UserInactivityLockedDay() - daysSinceLastLogin);
            int daysUntilDeleted = (int)(dateProvider.UserInactivityDeletionDay() - daysSinceLastLogin);
            _logger.LogInformation(
                "User {UserDisplayName} has been inactive for {DaysSinceLastLogin} days. They will be locked in {DaysUntilLocked} days and deleted in {DaysUntilDeleted} days.",
                user.DisplayName, daysSinceLastLogin, daysUntilLocked, daysUntilDeleted);

            if (lastLoginDate != null && emailValidator.IsValidEmail(user.Email))
            {
                _logger.LogInformation("Checking if the user needs to be notified at this time...");
                var email = await CheckIfUserToBeNotified(daysSinceLastLogin, daysUntilLocked, daysUntilDeleted, user.Email);

                if (email)
                {
                    _logger.LogInformation("User {UserDisplayName} needs to be notified. Sending email...", user.DisplayName);

                    if (dateProvider.UserInactivityNotificationDays().Contains(daysUntilLocked))
                    {
                        await notifyService.SendAccountLockingNoticeNotification(user.Email, daysSinceLastLogin.ToString(), daysUntilLocked.ToString());
                    }

                    if (dateProvider.UserInactivityNotificationDays().Contains(daysUntilDeleted))
                    {
                        await notifyService.SendAccountDeletionNoticeNotification(user.Email, daysSinceLastLogin.ToString(), daysUntilLocked.ToString());
                    }

                    _logger.LogInformation("Notification sent to {UserDisplayName} for inactivity, saving to db...", user.DisplayName);
                    await userInactivityNotificationService.AddInactivityNotification(user.Id, dateProvider.Today, daysUntilLocked, daysUntilDeleted, ct);
                }
            }
            if (daysUntilLocked <= 0)
            {
                await DisablePortalUser(user.Id);
            }
        }

        internal async Task DisablePortalUser(int portalUserId)
        {
            List<ProjectUserUpdateCommand> usersToUpdate = new();
            List<ProjectUserAddEntraUserCommand> usersToAdd = new();
            var projects = await projectUserManagementService.GetProjectListForPortalUser(portalUserId);
            foreach(var project in projects)
            {
                var projectUsers = await projectUserManagementService.GetProjectUsersAsync(project);
                var projectUser = projectUsers.Where(x => x.PortalUser.Id == portalUserId 
                    && x.Role.Id != (int)Project_Role.RoleNames.DisabledUser).FirstOrDefault();

                if (projectUser != null) // found not already disabled user
                {
                    var updateCommand = new ProjectUserUpdateCommand()
                    {
                        ProjectUser = projectUser,
                        NewRoleId = (int)Project_Role.RoleNames.DisabledUser
                    };
                    usersToUpdate.Add(updateCommand);
                }
            }
            await projectUserManagementService.ProcessProjectUserCommandsAsync(usersToUpdate, usersToAdd, portalUserId.ToString());

        }

        public async Task<bool> CheckIfUserToBeNotified(int daysSinceLastLogin, int daysUntilLocked, int daysUntilDeleted, string email)
        {
            if (dateProvider.UserInactivityNotificationDays().Contains(daysUntilLocked))
            {
                return true;
            }

            if (dateProvider.UserInactivityNotificationDays().Contains(daysUntilDeleted))
            {
                return true;
            }

            return false;
        }
    }
}