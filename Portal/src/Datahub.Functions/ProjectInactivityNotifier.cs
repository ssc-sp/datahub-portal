using System;
using System.Text.RegularExpressions;
using System.Text.Json;
using Datahub.Application.Services;
using Datahub.Application.Services.Projects;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Projects;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class ProjectInactivityNotifier
    {
        private readonly IMediator _mediator;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly int[] _notificationDays;
        private readonly int _deletionDay;
        private readonly ILogger<ProjectUsageNotifier> _logger;
        private readonly AzureConfig _config;
        private readonly QueuePongService _pongService;
        private readonly IProjectInactivityNotificationService _projectInactivityNotificationService;
        private readonly IResourceMessagingService _resourceMessagingService;

        public ProjectInactivityNotifier(ILoggerFactory loggerFactory, AzureConfig config, IMediator mediator, 
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, QueuePongService pongService, ProjectInactivityNotificationService projectInactivityNotificationService, IResourceMessagingService resourceMessagingService)
        {
            _logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();
            _mediator = mediator;
            _dbContextFactory = dbContextFactory;
            _notificationDays =
                ParseProjectInactivityNotificationDays(config.ProjectInactivityNotificationDays ?? "7,2");
            _deletionDay = ParseProjectInactivityDeletionDay(config.ProjectInactivityDeletionDays ?? "30");
            _pongService = pongService;
            _projectInactivityNotificationService = projectInactivityNotificationService;
            _resourceMessagingService = resourceMessagingService;
            _config = config;
        }

        [Function("ProjectInactivityNotifier")]
        public async Task Run([QueueTrigger("%QueueProjectInactivityNotification%", Connection = "DatahubStorageConnectionString")] string queueItem,
            CancellationToken ct)
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
            var project = await ctx.Projects.Where(x => x.Project_ID == message.ProjectId).FirstOrDefaultAsync(ct);
            
            // check if project exists and if we are past expiry date
            if (project != null  && project.ExpiryDate < DateTime.Today)
            {
                var lastLoginDate = project.LastLoginDate ?? project.Last_Updated_DT;
                var daysSinceLastLogin = (DateTime.Today - lastLoginDate).Days;
                var daysUntilDeletion = _deletionDay - daysSinceLastLogin;
                (var contacts, var acronym) = await GetProjectDetails(message.ProjectId, ct);

                // check if today is a threshold day
                if (_notificationDays.Contains(daysUntilDeletion))
                {
                
                    // get email object
                    var email = GetEmailRequestMessage(daysUntilDeletion, daysSinceLastLogin, acronym, contacts);
                
                    // send email
                    await _mediator.Send(email, ct);

                    // add notification to db
                    var sentTo = string.Join(",", contacts);
                    await _projectInactivityNotificationService.AddInactivityNotification(message.ProjectId, DateTime.Today, daysUntilDeletion, sentTo, ct);
                }
                // if its the last day of the list and the workspace is not whitelisted or if the workspace is whitelisted and today is the retirement date
                else if (daysSinceLastLogin >= _deletionDay && project.ExpiryDate < DateTime.Today)
                {
                    // send to terraform delete queue
                    var workspaceDefinition = await _resourceMessagingService.GetWorkspaceDefinition(acronym);
                    await _resourceMessagingService.SendToTerraformDeleteQueue(workspaceDefinition);
                }
            }
        }

        private int[] ParseProjectInactivityNotificationDays(string days)
        {
            return days.Split(",").Select(int.Parse).ToArray();
        }
        
        private int ParseProjectInactivityDeletionDay(string deletionDay)
        {
            return int.Parse(deletionDay);
        }
        
        private async Task<(List<string>,string)> GetProjectDetails( int projectId, CancellationToken cancellationToken)
        {
            var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            
            var project = await ctx.Projects
                .Where(e => e.Project_ID == projectId)
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
                return default;

            var contacts = project.Users
                .Select(u => u.PortalUser.Email)
                .Where(IsValidEmail)
                .ToList();

            return (contacts, project.Project_Acronym_CD);
        }

        private EmailRequestMessage GetEmailRequestMessage(int daysUntilDeletion, int daysSinceLastLogin, string acronym, List<string> contacts)
        {
            var (subjectTemplate, bodyTemplate) = TemplateUtils.GetEmailTemplate("project_inactive_alert.html");
            
            if (subjectTemplate is null || bodyTemplate is null)
                _logger.LogWarning("Email template file missing!");

            subjectTemplate ??= "Inactive workspace ({ws})";
            bodyTemplate ??= "<p>Your workspace ({ws}) has been inactive for {days} days.</p>";

            var subject = subjectTemplate
                .Replace("{ws}", acronym);

            var body = bodyTemplate
                .Replace("{ws}", acronym)
                .Replace("{inactive}", daysSinceLastLogin.ToString())
                .Replace("{remaining}", daysUntilDeletion.ToString());

            List<string> bcc = new() { GetNotificationCCAddress() };

            EmailRequestMessage notificationEmail = new() 
            { 
                To = contacts, 
                BccTo = bcc,
                Subject = subject, 
                Body = body 
            };
            return notificationEmail;
        }
        
        private string GetNotificationCCAddress()
        {
            return _config.Email?.NotificationsCCAddress ?? "fsdh-notifications-dhsf-notifications@ssc-spc.gc.ca";
        }
        
        static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        static ProjectInactivityNotificationMessage? DeserializeQueueMessage(string message)
        {
            return JsonSerializer.Deserialize<ProjectInactivityNotificationMessage>(message);
        }
    }
}