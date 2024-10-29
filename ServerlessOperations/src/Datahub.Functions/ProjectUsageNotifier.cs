using Azure.Messaging.ServiceBus;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Functions.Extensions;
using Datahub.Functions.Services;
using Datahub.Functions.Validators;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class ProjectUsageNotifier(
        ILoggerFactory loggerFactory,
        AzureConfig config,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        QueuePongService pongService,
        EmailValidator emailValidator,
        ISendEndpointProvider sendEndpointProvider,
        IEmailService emailService,
        IResourceMessagingService resourceMessagingService)
    {
        private readonly int[] _notificationPercents =
            ParseNotificationPercents(config.NotificationPercents ?? "25,50,80,100");

        private readonly ILogger<ProjectUsageNotifier> _logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();

        [Function("ProjectUsageNotifier")]
        public async Task Run(
            [ServiceBusTrigger(QueueConstants.ProjectUsageNotificationQueueName,
                Connection = "DatahubServiceBus:ConnectionString")]
            ServiceBusReceivedMessage serviceBusReceivedMessage,
            CancellationToken cancellationToken)
        {
            // test for ping
            // if (await pongService.Pong(serviceBusReceivedMessage.Body.ToString()))
                // return;

            // deserialize message
            var message = await serviceBusReceivedMessage.DeserializeAndUnwrapMessageAsync<ProjectUsageNotificationMessage>();

            // verify message 
            if (message is null || message.ProjectAcronym.Length <= 0)
            {
                throw new Exception($"Invalid queue message:\n{serviceBusReceivedMessage.Body}");
            }

            // run project verification
            await VerifyAndNotifyProject(message.ProjectAcronym, cancellationToken);
            
            await VerifyOverBudgetIsDeleted(message.ProjectAcronym, cancellationToken);
        }

        /// <summary>
        /// Verifies that any over-budget records associated with the specified project acronym
        /// are set to deleted in the database and sent to the service bus otherwise.
        /// </summary>
        /// <param name="projectAcronym">The project acronym associated with the message.</param>
        /// <param name="cancellationToken">A cancellation token to observe during the task execution.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal async Task VerifyOverBudgetIsDeleted(string projectAcronym, CancellationToken cancellationToken)
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var deleteIsRequired = await VerifyDeleteIsRequired(projectAcronym, cancellationToken, ctx);
            
            if(!deleteIsRequired)
                return;

            var undeletedResources = await ctx.Project_Resources2
                .Include(r => r.Project)
                .Where(r => r.Project.Project_Acronym_CD == projectAcronym && !(r.Status == TerraformStatus.Deleted || r.Status == TerraformStatus.DeleteRequested))
                .ToListAsync(cancellationToken);
            
            if(undeletedResources.Count == 0)
                return;

            foreach (var undeletedResource in undeletedResources)
            {
                undeletedResource.Status = TerraformStatus.DeleteRequested;
                ctx.Project_Resources2.Update(undeletedResource);
            }
            await ctx.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Project {ProjectId} is over budget, deleting resources", projectAcronym);
            var workspaceDefinition = await resourceMessagingService.GetWorkspaceDefinition(projectAcronym);
            await resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
            _logger.LogInformation("Project {ProjectId} resources have been queued for deletion", projectAcronym);
        }

        /// <summary>
        /// Determines if a delete action is required for the specified project acronym.
        /// </summary>
        /// <param name="projectAcronym">The acronym of the project to check.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="ctx">The database context used for accessing project data.</param>
        /// <returns>A boolean indicating whether delete is required.</returns>
        internal async Task<bool> VerifyDeleteIsRequired(string projectAcronym, CancellationToken cancellationToken,
            DatahubProjectDBContext ctx)
        {
            var details = await GetProjectDetails(ctx, projectAcronym, cancellationToken);
            
            if (details?.Credits is null)
            {
                _logger.LogWarning("Project {ProjectId} details or credits are null", projectAcronym);
                return false;
            }
            
            var currentPercent = details.ProjectBudget > 0
                ? (int)Math.Round(100.0 * details.Credits.Current / details.ProjectBudget)
                : 0;

            if (currentPercent < 100)
            {
                _logger.LogInformation("Project {ProjectId} is not over budget", projectAcronym);
                return false;
            }
            
            var project = await ctx.Projects
                .AsNoTracking()
                .Where(e => e.Project_Acronym_CD == projectAcronym)
                .FirstAsync(cancellationToken);
            
            if(project.PreventAutoDelete)
            {
                _logger.LogInformation("Project {ProjectId} is over budget but auto-delete is disabled", projectAcronym);
                return false;
            }

            return true;
        }

        private async Task VerifyAndNotifyProject(string projectAcronym, CancellationToken cancellationToken)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            // load from details from db
            var details = await GetProjectDetails(ctx, projectAcronym, cancellationToken);

            if (details?.Credits is null)
            {
                // log that details credits are null
                _logger.LogWarning("Project {ProjectId} details or credits are null", projectAcronym);
                return;
            }

            // calc current %
            var currentPercent = details.ProjectBudget > 0
                ? (int)Math.Round(100.0 * details.Credits.Current / details.ProjectBudget)
                : 0;

            // get the matching notification %
            var notificationPerc = GetNotificationPercent(currentPercent);

            // check if notification is not needed 
            if (notificationPerc == 0 || details.Credits.PercNotified == notificationPerc)
            {
                return;
            }

            try
            {
                var notificationEmail = GetNotificationEmail(details.ProjectAcro, notificationPerc, details.Contacts);
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.EmailNotificationQueueName,
                    notificationEmail);

                details.Credits.PercNotified = notificationPerc;
                details.Credits.LastNotified = DateTime.UtcNow;

                ctx.Project_Credits.Update(details.Credits);

                await ctx.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Notifiying {0}% consumed for workspace {1}", notificationPerc, details.ProjectAcro);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "The ProjectUsageNotifier was unable the update the DB or send the email, check the next log.");
                _logger.LogError(ex.Message, ex);
            }
        }

        private async Task<ProjectNotificationDetails?> GetProjectDetails(DatahubProjectDBContext ctx,
            string projectAcronym,
            CancellationToken cancellationToken)
        {
            var project = await ctx.Projects
                .Where(e => e.Project_Acronym_CD == projectAcronym)
                .Include(e => e.Credits)
                .Include(e => e.Users)
                .ThenInclude(e => e.PortalUser)
                .AsSingleQuery()
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
                return default;

            var contacts = project.Users
                .Select(u => u.PortalUser.Email)
                .Where(emailValidator.IsValidEmail)
                .ToList();

            var budget = Convert.ToDouble(project.Project_Budget);

            return new(project.Project_Acronym_CD, contacts, budget, project.Credits);
        }

        private EmailRequestMessage GetNotificationEmail(string projectAcro, int perc, List<string> contacts)
        {
            Dictionary<string, string> bodyArgs = new()
            {
                { "{ws}", projectAcro },
                { "{perc}", perc.ToString() }
            };

            Dictionary<string, string> subjectArgs = new()
            {
                { "{ws}", projectAcro },
                { "{perc}", perc.ToString() }
            };

            List<string> bcc = new();
            if (perc > 70) // Only sends the notification to us for the 75% and 100% notifications
            {
                bcc.Add(GetNotificationCCAddress());
            }

            return emailService.BuildEmail("cost_alert.html", contacts, bcc, bodyArgs, subjectArgs);
        }

        private string GetNotificationCCAddress()
        {
            return config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
        }

        private int GetNotificationPercent(int value)
        {
            return _notificationPercents.OrderByDescending(v => v).FirstOrDefault(v => value >= v);
        }

        static int[] ParseNotificationPercents(string percents)
        {
            return percents.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim()))
                .ToArray();
        }

        record ProjectNotificationDetails(
            string ProjectAcro,
            List<string> Contacts,
            double ProjectBudget,
            Project_Credits Credits);
    }
}