using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Context;
using Datahub.Functions.Extensions;
using Datahub.Functions.Providers;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages;
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
        IGCNotifyService gcNotifyService)
    {
        private readonly ILogger<ProjectUsageNotifier> _logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();

        [Function("ProjectInactivityNotifier")]
        public async Task Run(
            [ServiceBusTrigger(QueueConstants.ProjectInactivityNotificationQueueName,
                Connection = "DatahubServiceBus:ConnectionString")]
            ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken ct)
        {
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
        
            if (await CheckIfProjectToBeNotified(daysUntilDeletion, daysSinceLastLogin, operationalWindow, acronym, contacts))
            {
                var adminContact = new List<string>() { config.Email.AdminEmail };
                await gcNotifyService.SendWorkspaceInactiveNotification(adminContact[0], daysSinceLastLogin.ToString());

                foreach (string contact in contacts)
                {
                    _logger.LogInformation("Sending inactivity notification to {Contact} for project {Acronym}...", contact, acronym);
                    await gcNotifyService.SendWorkspaceInactiveNotification(contact, daysSinceLastLogin.ToString());

                    _logger.LogInformation("Inactivity notification sent to {Contact} for project {Acronym}, saving to db...", contact, acronym);
                    await projectInactivityNotificationService.AddInactivityNotification(message.ProjectId, dateProvider.Today, daysUntilDeletion, contact, ct);
                }
            }
            else
            {
                _logger.LogInformation("Project {Acronym} (ID: {ProjectId}) is not due for inactivity notification.", acronym, message.ProjectId);
            }
        }

        public bool CheckIfProjectToBeNotified(int daysUntilDeletion,
            int daysSinceLastLogin, DateTime? operationalWindow, string acronym,
            List<string> contacts)
        {
            // check if we are past operational window or that it is null and that the project has no cost recovery and that
            if ((operationalWindow == null || operationalWindow < dateProvider.Today) &&
                dateProvider.ProjectNotificationDays().Contains(daysUntilDeletion))
            {
                return true;
            }

            return false;
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
    }
}