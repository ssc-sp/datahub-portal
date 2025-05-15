using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Shared.Configuration;
using MassTransit;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
	public class InactivityScheduler(
        ILoggerFactory loggerFactory,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        ISendEndpointProvider sendEndpointProvider)
    {
        
        private readonly ILogger<InactivityScheduler> _logger = loggerFactory.CreateLogger<InactivityScheduler>();

        [Function("InactivityScheduler")]
        public async Task Run([TimerTrigger("%InactivityCRON%")] TimerInfo timerInfo)
        {
            await ScheduleProjects();
            await ScheduleUsers();

        }

#if DEBUG
        [Function("InactivitySchedulerHttp")]
        public async Task RunHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
        {
            await ScheduleProjects();
            await ScheduleUsers();
        }
#endif

        internal virtual async Task ScheduleProjects()
        {
            _logger.LogInformation("Getting projects to schedule for inactivity notifications");
            var projects = await GetProjects();
            
            _logger.LogInformation($"Found {projects.Count} projects to schedule for inactivity notifications");
            foreach (var project in projects)
            {
                var message = DeserializeProjectMessage(project);
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ProjectInactivityNotificationQueueName, message);
            }
            _logger.LogInformation($"Scheduled {projects.Count} projects for inactivity notifications");
        }
        
        internal virtual async Task ScheduleUsers()
        {
            _logger.LogInformation("Getting users to schedule for inactivity notifications");
            var users = await GetUsers();

            _logger.LogInformation($"Found {users.Count} users to schedule for inactivity notifications");
            foreach (var user in users)
            {
                var message = DeserializeUserMessage(user);
                await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.UserInactivityNotification, message);
            }
            _logger.LogInformation($"Scheduled {users.Count} users for inactivity notifications");
        }

        private async Task<List<int>> GetProjects()
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();

            return await ctx.Projects
                .AsNoTracking()
                .AsAsyncEnumerable()
                .Where(p => !p.IsDeleted)
                .Select(p => p.Project_ID)
                .Distinct()
                .ToListAsync();
            //return ctx.Projects.Where(w => !w.IsDeleted).AsNoTracking().Select(x => x.Project_ID).Distinct().ToList();
        }

        private async Task<List<int>> GetUsers()
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            return ctx.PortalUsers.AsNoTracking().Select(x => x.Id).Distinct().ToList();
        }

        private ProjectInactivityNotificationMessage DeserializeProjectMessage(int projectId)
        {
            return new ProjectInactivityNotificationMessage(projectId);
        }

        private UserInactivityNotificationMessage DeserializeUserMessage(int userId)
        {
            return new UserInactivityNotificationMessage(userId);
        }
    }
    
}