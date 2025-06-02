using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
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
        QueuePongService pongService,
        EmailValidator emailValidator,
        IUserInactivityNotificationService userInactivityNotificationService,
        ISendEndpointProvider sendEndpointProvider,
        IEmailService emailService)
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
            var daysSinceLastLogin = (dateProvider.Today - lastLoginDate)?.Days;
            var daysUntilLocked = dateProvider.UserInactivityLockedDay() - daysSinceLastLogin;
            var daysUntilDeleted = dateProvider.UserInactivityDeletionDay() - daysSinceLastLogin;
            _logger.LogInformation(
                "User {UserDisplayName} has been inactive for {DaysSinceLastLogin} days. They will be locked in {DaysUntilLocked} days and deleted in {DaysUntilDeleted} days.",
                user.DisplayName, daysSinceLastLogin, daysUntilLocked, daysUntilDeleted);

            if (lastLoginDate != null && emailValidator.IsValidEmail(user.Email))
            {
                _logger.LogInformation("Checking if the user needs to be notified at this time...");
                var email = await CheckIfUserToBeNotified(daysSinceLastLogin!.Value, daysUntilLocked!.Value,
                    daysUntilDeleted!.Value, user.Email);

                if (email != null)
                {
                    _logger.LogInformation("User {UserDisplayName} needs to be notified. Sending email...",
                        user.DisplayName);
                    await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName,
                        email, ct);

                    // send notification to db
                    _logger.LogInformation("Notification sent to {UserDisplayName} for inactivity, saving to db...",
                        user.DisplayName);
                    await userInactivityNotificationService.AddInactivityNotification(user.Id, dateProvider.Today,
                        daysUntilLocked!.Value, daysUntilDeleted!.Value, ct);
                    _logger.LogInformation("Notification saved to db for {UserDisplayName}", user.DisplayName);
                }
            }
        }

        public async Task<EmailRequestMessage?> CheckIfUserToBeNotified(int daysSinceLastLogin, int daysUntilLocked,
            int daysUntilDeleted, string email)
        {
            if (dateProvider.UserInactivityNotificationDays().Contains(daysUntilLocked))
            {
                return GetLockedEmailRequestMessage(daysSinceLastLogin, daysUntilLocked, email);
            }

            if (dateProvider.UserInactivityNotificationDays().Contains(daysUntilDeleted))
            {
                return GetDeletedEmailRequestMessage(daysSinceLastLogin, daysUntilDeleted, email);
            }

            return null;
        }

        public EmailRequestMessage GetLockedEmailRequestMessage(int daysSinceLastLogin, int daysUntilLocked,
            string email)
        {
            return GetEmailRequestMessage(daysSinceLastLogin, daysUntilLocked, "user_lock", email);
        }

        public EmailRequestMessage GetDeletedEmailRequestMessage(int daysSinceLastLogin, int daysUntilDeleted,
            string email)
        {
            return GetEmailRequestMessage(daysSinceLastLogin, daysUntilDeleted, "user_deletion", email);
        }

        public EmailRequestMessage GetEmailRequestMessage(int daysSince, int daysUntil, string reason, string email)
        {
            Dictionary<string, string> bodyArgs = new()
            {
                { "{daysSince}", daysSince.ToString() },
                { "{daysUntil}", daysUntil.ToString() }
            };

            Dictionary<string, string> subjectArgs = new();

            List<string> bcc = new() { GetNotificationCCAddress() };

            return emailService.BuildEmail($"{reason}_alert.html", new List<string>() { email }, bcc, bodyArgs,
                subjectArgs);
        }

        private string GetNotificationCCAddress()
        {
            return config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
        }
    }
}