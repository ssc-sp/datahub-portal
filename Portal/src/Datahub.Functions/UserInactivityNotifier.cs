using System.Linq.Dynamic.Core;
using System.Text.Json;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Functions.Providers;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
	public class UserInactivityNotifier
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UserInactivityNotifier> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IUserInactivityNotificationService _userInactivityNotificationService;
        private readonly IEmailService _emailService;
        private readonly IDateProvider _dateProvider;
        private readonly AzureConfig _config;

        private readonly QueuePongService _pongService;
        private readonly EmailValidator _emailValidator;

        public UserInactivityNotifier(IPublishEndpoint publishEndpoint, ILoggerFactory loggerFactory,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IDateProvider dateProvider, AzureConfig config,
            QueuePongService pongService, EmailValidator emailValidator, IUserInactivityNotificationService userInactivityNotificationService, IEmailService emailService)
        {
            _publishEndpoint = publishEndpoint;
            _logger = loggerFactory.CreateLogger<UserInactivityNotifier>();
            _dbContextFactory = dbContextFactory;
            _dateProvider = dateProvider;
            _config = config;
            _pongService = pongService;
            _emailValidator = emailValidator;
            _userInactivityNotificationService = userInactivityNotificationService;
            _emailService = emailService;
        }

        [Function("UserInactivityNotifier")]
        public async Task Run(
            [QueueTrigger("%QueueUserInactivityNotification%", Connection = "DatahubStorageConnectionString")]
            string queueItem, CancellationToken ct)
        {
            // test for ping
            if (await _pongService.Pong(queueItem))
                return;

            // deserialize message
            var message = DeserializeQueueMessage(queueItem);

            // verify message 
            if (message is null)
            {
                throw new Exception($"Invalid queue message:\n{queueItem}");
            }

            using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

            // get project
            var user = await ctx.PortalUsers.AsNoTracking().Where(x => x.Id == message.UserId).FirstOrDefaultAsync(ct);

            var lastLoginDate = user.LastLoginDateTime ?? user.FirstLoginDateTime;
            var daysSinceLastLogin = (_dateProvider.Today - lastLoginDate)?.Days;
            var daysUntilLocked = _dateProvider.UserInactivityLockedDay() - daysSinceLastLogin;
            var daysUntilDeleted = _dateProvider.UserInactivityDeletionDay() - daysSinceLastLogin;

            if (lastLoginDate != null && _emailValidator.IsValidEmail(user.Email))
            {
                var email = await CheckIfUserToBeNotified(daysSinceLastLogin!.Value, daysUntilLocked!.Value,
                    daysUntilDeleted!.Value, user.Email);

                if (email != null)
                {
                    await _publishEndpoint.Publish(email, ct);
                    
                    // send notification to db
                    await _userInactivityNotificationService.AddInactivityNotification(user.Id, _dateProvider.Today,
                        daysUntilLocked!.Value, daysUntilDeleted!.Value, ct);
                }
            }
        }

        public async Task<EmailRequestMessage?> CheckIfUserToBeNotified(int daysSinceLastLogin, int daysUntilLocked,
            int daysUntilDeleted, string email)
        {
            if (_dateProvider.UserInactivityNotificationDays().Contains(daysUntilLocked))
            {
                return GetLockedEmailRequestMessage(daysSinceLastLogin, daysUntilLocked, email);
            }

            if (_dateProvider.UserInactivityNotificationDays().Contains(daysUntilDeleted))
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

            return _emailService.BuildEmail($"{reason}_alert.html", new List<string>() { email }, bcc, bodyArgs, subjectArgs);
        }

        private string GetNotificationCCAddress()
        {
            return _config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
        }

        static UserInactivityNotificationMessage? DeserializeQueueMessage(string queueItem)
        {
            return JsonSerializer.Deserialize<UserInactivityNotificationMessage>(queueItem);
        }
    }
}