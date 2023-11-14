using System;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Datahub;
using Datahub.Functions.Providers;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class UserInactivityNotifier
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserInactivityNotifier> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IUserInactivityNotificationService _userInactivityNotificationService;
        private readonly IDateProvider _dateProvider;
        private readonly AzureConfig _config;

        private readonly QueuePongService _pongService;
        private readonly EmailValidator _emailValidator;

        public UserInactivityNotifier(IMediator mediator, ILoggerFactory loggerFactory,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IDateProvider dateProvider, AzureConfig config,
            QueuePongService pongService, EmailValidator emailValidator)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<UserInactivityNotifier>();
            _dbContextFactory = dbContextFactory;
            _dateProvider = dateProvider;
            _config = config;
            _pongService = pongService;
            _emailValidator = emailValidator;
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
                    await _mediator.Send(email, ct);
                    
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
            var (subjectTemplate, bodyTemplate) = TemplateUtils.GetEmailTemplate($"{reason}_alert.html");

            if (subjectTemplate is null || bodyTemplate is null)
                _logger.LogWarning("Email template file missing!");

            subjectTemplate ??= "FSDH - User inactivity";
            string reasonTemplate = reason == "user_lock" ? "locked" : "deleted";
            bodyTemplate ??=
                $"<p>Your account has been inactive for {{daysSince}} days. Due to our cloud user policies, your account will be {reasonTemplate} in {{daysUntil}} day(s) if you do not login to the FSDH.</p>";


            var body = bodyTemplate
                .Replace("{daysSince}", daysSince.ToString())
                .Replace("{daysUntil}", daysUntil.ToString());

            List<string> bcc = new() { GetNotificationCCAddress() };

            EmailRequestMessage notificationEmail = new()
            {
                To = new List<string>() { email },
                BccTo = bcc,
                Subject = subjectTemplate,
                Body = body
            };

            return notificationEmail;
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