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
        IQueuePongService pongService,
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
            _logger.LogInformation("Deserializing project inactivity notification message...");
            var message = await serviceBusReceivedMessage
                .DeserializeAndUnwrapMessageAsync<ProjectInactivityNotificationMessage>();

            // verify message 
            if (message is null)
            {
                _logger.LogError("Invalid queue message: {MessageBody}", serviceBusReceivedMessage.Body.ToString());
                throw new Exception($"Invalid queue message:\n{serviceBusReceivedMessage.Body.ToString()}");
            }

            _logger.LogInformation("Received project inactivity notification for ProjectId: {ProjectId}", message.ProjectId);
        
            await using var ctx = await dbContextFactory.CreateDbContextAsync(ct);

            // get project
            var project = await ctx.Projects
                .Include(p => p.UserRoles)
                .ThenInclude(u => u.PortalUser)
                .AsNoTracking()
                .Where(x => x.Project_ID == message.ProjectId)
                .FirstOrDefaultAsync(ct);

            if (project is null)
            {
                _logger.LogWarning("Project with ID {ProjectId} not found.", message.ProjectId);
                throw new InvalidDataException($"Project with ID {message.ProjectId} not found.");
            }
        
            // get project info
            var lastLoginDate = project?.LastLoginDate ?? project.Last_Updated_DT;
            var daysSinceLastLogin = (dateProvider.Today - lastLoginDate).Days;
            var daysUntilDeletion = dateProvider.ProjectSoftDeletionDay() - daysSinceLastLogin;
            var operationalWindow = project.OperationalWindow;
            var (contacts, acronym) = await GetProjectDetails(message.ProjectId, ct);

            _logger.LogInformation("Project {Acronym} (ID: {ProjectId}) last activity: {LastLoginDate}, inactive for {DaysSinceLastLogin} days, {DaysUntilDeletion} days until soft deletion.",
                acronym, message.ProjectId, lastLoginDate, daysSinceLastLogin, daysUntilDeletion);
        
            //var adminContact = new List<string>() { "datasolutions-solutiondedonnees@ssc-spc.gc.ca" };
            var adminContact = new List<string>() { config.Email.AdminEmail };

            // check if project to be notified
            _logger.LogInformation("Checking if project {Acronym} needs to be notified for inactivity...", acronym);
            var email = await CheckIfProjectToBeNotified(daysUntilDeletion, daysSinceLastLogin, operationalWindow,
                 acronym, contacts);

            var adminEmailBodyText = await GetAdminEmailBodyText(daysSinceLastLogin, acronym);

            var emailForAdmin = GetEmailRequestMessage(daysUntilDeletion, daysSinceLastLogin, acronym, adminContact, "project_inactive_alert_dhadmin.html", adminEmailBodyText);
        
            // if email is not null, send email
            if (email != null)
            {
                _logger.LogInformation("Project leads for {Acronym} need to be notified. Sending email...", acronym);
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName,email, ct);
        
                // add notification to db
                var sentTo = string.Join(",", contacts);
                _logger.LogInformation("Notification sent to project leads ({SentTo}) for project {Acronym}, saving to db...", sentTo, acronym);
                await projectInactivityNotificationService.AddInactivityNotification(message.ProjectId, dateProvider.Today, daysUntilDeletion, sentTo, ct);

                //notify admin to follow up
                if (emailForAdmin != null)
                {
                    _logger.LogInformation("Notifying admin for project {Acronym} inactivity...", acronym);
                    await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName, emailForAdmin, ct);
                    sentTo = adminContact[0];
                    _logger.LogInformation("Admin notification sent for project {Acronym}, saving to db...", acronym);
                    await projectInactivityNotificationService.AddInactivityNotification(message.ProjectId, dateProvider.Today, daysUntilDeletion, sentTo, ct);
                }
            }
            else if (emailForAdmin != null && daysSinceLastLogin > dateProvider.ProjectSoftDeletionDay() && IsTodayMonday())
            {
                _logger.LogInformation("Project {Acronym} past soft deletion day and today is Monday. Notifying admin...", acronym);
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName, emailForAdmin, ct);
                var sentTo = adminContact[0];
                _logger.LogInformation("Admin notification sent for project {Acronym}, saving to db...", acronym);
                await projectInactivityNotificationService.AddInactivityNotification(message.ProjectId, dateProvider.Today, daysUntilDeletion, sentTo, ct);
            }
        }

        private async Task<(string, string)> GetAdminEmailBodyText(int daysSinceLastLogin, string acronym)
        {
            (string, string) bodyText = ("", "");
            if (daysSinceLastLogin > dateProvider.ProjectHardDeletionDay())
            {
                bodyText = ($"The workspace <a href=\"https://federal-science-datahub.canada.ca/w/{acronym}\">{acronym}</a> has been inactive for over {dateProvider.ProjectHardDeletionDay().ToString()} days. Assuming the workspace leads are unreachable, please consider deleting the workspace.",
                            $"L'espace de travail <a href=\"https://federal-science-datahub.canada.ca/w/{acronym}\">{acronym}</a> est inactif depuis plus de {dateProvider.ProjectHardDeletionDay().ToString()} jours. En supposant que les responsables de l'espace de travail soient injoignables, veuillez envisager de supprimer l'espace de travail.");
            }
            else if (daysSinceLastLogin > dateProvider.ProjectSoftDeletionDay())
            {
                bodyText = ($"The workspace <a href=\"https://federal-science-datahub.canada.ca/w/{acronym}\">{acronym}</a> has been inactive for over {dateProvider.ProjectSoftDeletionDay().ToString()} days. Please contact the workspace leads to determine if the workspace can be deleted.",
                    $"L'espace de travail <a href=\" https://federal-science-datahub.canada.ca/w/{acronym}\">{acronym}</a> est inactif depuis plus de {dateProvider.ProjectSoftDeletionDay().ToString()} jours. Veuillez contacter les responsables de l'espace de travail pour déterminer si l'espace de travail peut être supprimé.");
            }
            else
            {
                bodyText = ($"The workspace <a href=\"https://federal-science-datahub.canada.ca/w/{acronym}\">{acronym}</a> has been inactive for {daysSinceLastLogin} days. The workspace leads have been alerted to login to the workspace to prevent the workspace from potentially being deleted.",
                            $"L'espace de travail <a href=« https://federal-science-datahub.canada.ca/w/{acronym}\\ »>{acronym}</a> est inactif depuis {daysSinceLastLogin} jours. Les responsables de l'espace de travail ont été invités à se connecter à l'espace de travail pour éviter qu'il ne soit supprimé.");
            }
            return bodyText;
        }

        public async Task<EmailRequestMessage?> CheckIfProjectToBeNotified(int daysUntilDeletion,
            int daysSinceLastLogin, DateTime? operationalWindow, string acronym,
            List<string> contacts)
        {
            // check if we are past operational window or that it is null and that the project has no cost recovery and that
            if ((operationalWindow == null || operationalWindow < dateProvider.Today) &&
                dateProvider.ProjectNotificationDays().Contains(daysUntilDeletion))
            {
                return GetEmailRequestMessage(daysUntilDeletion, daysSinceLastLogin, acronym, contacts, "project_inactive_alert.html", (string.Empty, string.Empty));
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
                   daysSinceLastLogin >= dateProvider.ProjectSoftDeletionDay() &&
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

            var contacts = project.UserRoles?
                .Select(u => u.PortalUser.Email)
                .Where(emailValidator.IsValidEmail)
                .ToList();

            return (contacts, project.Project_Acronym_CD);
        }

        public EmailRequestMessage GetEmailRequestMessage(int daysUntilDeletion, int daysSinceLastLogin,
            string acronym, List<string> contacts, string template, (string, string) bodytext)
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
                { "{remaining}", daysUntilDeletion.ToString() },
                { "{bodytext}", bodytext.Item1 },
                { "{bodytextFr}", bodytext.Item2 },
            };

            var email = emailService.BuildEmail(template, contacts, bcc, bodyArgs,
                subjectArgs);

            return email;
        }

        private string GetNotificationCCAddress()
        {
            return config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
        }
        private bool IsTodayMonday()
        {
            return DateTime.Now.DayOfWeek == DayOfWeek.Monday;
        }
    }
}