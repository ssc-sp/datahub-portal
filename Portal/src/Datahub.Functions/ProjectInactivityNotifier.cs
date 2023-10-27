using System;
using System.Text.RegularExpressions;
using System.Text.Json;
using Datahub.Core.Model.Datahub;
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
        private readonly ILogger<ProjectUsageNotifier> _logger;
        private readonly AzureConfig _config;
        private readonly QueuePongService _pongService;
        private readonly ProjectInactivityService _inactivityService;

        public ProjectInactivityNotifier(ILoggerFactory loggerFactory, AzureConfig config, IMediator mediator, 
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, QueuePongService pongService, ProjectInactivityService inactivityService)
        {
            _logger = loggerFactory.CreateLogger<ProjectUsageNotifier>();
            _mediator = mediator;
            _dbContextFactory = dbContextFactory;
            _notificationDays =
                ParseProjectInactivityNotificationDays(config.ProjectInactivityNotificationDays ?? "23,28,30");
            _pongService = pongService;
            _config = config;
            _inactivityService = inactivityService;
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
            
            // get threshold
            var threshold = GetThreshold(message.LastLogin);

            // check if the threshold is not the last days in the list and send email unless the workspace is whitelisted
            if ((threshold is not null) && (threshold != _notificationDays[-1]) && !message.Whitelisted)
            {
                using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);
                (var contacts, var acronym) = await GetProjectDetails(ctx, message.ProjectId, ct);
                
                // non-null threshold
                var nthreshold = threshold ?? 0;
                
                // get email object
                var email = GetEmailRequestMessage(nthreshold, acronym, contacts);
                
                // send email
                await _mediator.Send(email, ct);
                
                // update db
                var today = DateTime.Now;
                await _inactivityService.SetProjectThresholdNotified(message.ProjectId, nthreshold, ct);
                await _inactivityService.SetProjectDateLastNotified(message.ProjectId, today, ct);
            }
            // if its the last day of the list and the workspace is not whitelisted or if the workspace is whitelisted and today is the retirement date
            else if (((threshold is not null) && (threshold != _notificationDays[-1]) && !message.Whitelisted) || (message.Whitelisted && message.RetirementDate == DateTime.Today))
            {
                // last threshold
                var nthreshold = threshold ?? 0;
                
                // TODO: delete workspace
            }
        }

        private int[] ParseProjectInactivityNotificationDays(string days)
        {
            return days.Split(",").Select(int.Parse).ToArray();
        }
        
        private async Task<(List<string>,string)> GetProjectDetails(DatahubProjectDBContext ctx, int projectId, CancellationToken cancellationToken)
        {
            var project = await ctx.Projects
                .Where(e => e.Project_ID == projectId)
                .Include(e => e.Credits)
                .Include(e => e.Users)
                .AsSingleQuery()
                .FirstOrDefaultAsync(cancellationToken);

            if (project is null)
                return default;

            var contacts = project.Users
                .Select(u => u.User_Name)
                .Where(IsValidEmail)
                .ToList();

            return (contacts, project.Project_Acronym_CD);
        }

        private int? GetThreshold(int lastLogin)
        {
            foreach (var t in _notificationDays)
            {
                if (lastLogin == t)
                    return t;
            }

            return null;
        }

        private EmailRequestMessage GetEmailRequestMessage(int threshold, string acronym, List<string> contacts)
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
                .Replace("{days}", threshold.ToString())
                .Replace("{remaining}", (_notificationDays[-1] - threshold).ToString());

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