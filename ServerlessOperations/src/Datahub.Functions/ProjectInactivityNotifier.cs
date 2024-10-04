using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
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
using Datahub.Shared.Entities;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class ProjectInactivityNotifier(
        ILoggerFactory loggerFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        QueuePongService pongService,
        ISendEndpointProvider sendEndpointProvider,
        IProjectInactivityNotificationService projectInactivityNotificationService,
        EmailValidator emailValidator,
        IDateProvider dateProvider,
        AzureConfig config,
        IEmailService emailService)
    {
        private readonly ILogger<ProjectUsageNotifier> _logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();

        [Function("ProjectInactivityNotifier")]
        public async Task Run(
            [ServiceBusTrigger(QueueConstants.ProjectInactivityNotificationQueueName,
                Connection = "DatahubServiceBus:ConnectionString")]
            ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken ct)
        {
            // test for ping
            // if (await pongService.Pong(serviceBusReceivedMessage.Body.ToString()))
            // return;

            // deserialize message

            var message = await serviceBusReceivedMessage
                .DeserializeAndUnwrapMessageAsync<ProjectInactivityNotificationMessage>();

            // verify message 
            if (message is null)
            {
                throw new Exception($"Invalid queue message:\n{serviceBusReceivedMessage.Body.ToString()}");
            }

            await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

            // get project
            var project = await ctx.Projects.AsNoTracking().Where(x => x.Project_ID == message.ProjectId)
                .FirstOrDefaultAsync(ct);

            // get project info
            var lastLoginDate = project?.LastLoginDate ?? project.Last_Updated_DT;
            var daysSinceLastLogin = (dateProvider.Today - lastLoginDate).Days;
            var daysUntilDeletion = dateProvider.ProjectDeletionDay() - daysSinceLastLogin;
            var operationalWindow = project.OperationalWindow;
            var hasCostRecovery = project.HasCostRecovery;
            var (contacts, acronym) = await GetProjectDetails(message.ProjectId, ct);

            // check if project to be notified
            var email = await CheckIfProjectToBeNotified(daysUntilDeletion, daysSinceLastLogin, operationalWindow,
                hasCostRecovery, acronym, contacts);

            // if email is not null, send email
            if (email != null)
            {
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName,
                    email, ct);

                // add notification to db
                var sentTo = string.Join(",", contacts);
                await projectInactivityNotificationService.AddInactivityNotification(message.ProjectId,
                    dateProvider.Today, daysUntilDeletion, sentTo, ct);
            }

            // check if project to be deleted
            var projectToBeDeleted = CheckIfProjectToBeDeleted(daysSinceLastLogin, operationalWindow, hasCostRecovery);

            // if project to be deleted, send to terraform delete queue
            if (projectToBeDeleted)
            {
                var projectInactiveMessage = new ProjectInactiveMessage(acronym);
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectInactiveQueueName, projectInactiveMessage,
                    ct);
            }
        }

        public async Task<EmailRequestMessage?> CheckIfProjectToBeNotified(int daysUntilDeletion,
            int daysSinceLastLogin, DateTime? operationalWindow, bool hasCostRecovery, string acronym,
            List<string> contacts)
        {
            // check if we are past operational window or that it is null and that the project has no cost recovery and that
            if ((operationalWindow == null || operationalWindow < dateProvider.Today) && !hasCostRecovery &&
                dateProvider.ProjectNotificationDays().Contains(daysUntilDeletion))
            {
                return GetEmailRequestMessage(daysUntilDeletion, daysSinceLastLogin, acronym, contacts);
            }

            return null;
        }

        public bool CheckIfProjectToBeDeleted(int daysSinceLastLogin,
            DateTime? operationalWindow, bool hasCostRecovery)
        {
            // check if we are past operational window or that it is null
            // and that the project has no cost recovery
            // and that we are at or are past the deletion day
            return (operationalWindow == null || operationalWindow < dateProvider.Today) &&
                   daysSinceLastLogin >= dateProvider.ProjectDeletionDay() &&
                   !hasCostRecovery;
        }

        private async Task<(List<string>, string)> GetProjectDetails(int projectId, CancellationToken cancellationToken)
        {
            var ctx = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var project = await ctx.Projects
                .AsNoTracking()
                .Where(e => e.Project_ID == projectId)
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
                return default;

            var contacts = project.Users?
                .Select(u => u.PortalUser.Email)
                .Where(emailValidator.IsValidEmail)
                .ToList();

            return (contacts, project.Project_Acronym_CD);
        }

        public EmailRequestMessage GetEmailRequestMessage(int daysUntilDeletion, int daysSinceLastLogin,
            string acronym, List<string> contacts)
        {
            List<string> bcc = new() { GetNotificationCCAddress() };

            Dictionary<string, string> subjectArgs = new()
            {
                { "{{ws}}", acronym }
            };

            Dictionary<string, string> bodyArgs = new()
            {
                { "{ws}", acronym },
                { "{inactive}", daysSinceLastLogin.ToString() },
                { "{remaining}", daysUntilDeletion.ToString() }
            };

            var email = emailService.BuildEmail("project_inactive_alert.html", contacts, bcc, bodyArgs,
                subjectArgs);

            return email;
        }

        private string GetNotificationCCAddress()
        {
            return config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
        }
    }
}