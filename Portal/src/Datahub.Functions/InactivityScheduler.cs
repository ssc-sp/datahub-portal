using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
	public class InactivityScheduler
    {
        
        private readonly ILogger<InactivityScheduler> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IMediator _mediator;

        public InactivityScheduler(ILoggerFactory loggerFactory, IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IMediator mediator)
        {
            _logger = loggerFactory.CreateLogger<InactivityScheduler>();
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
        }
        
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

        private async Task ScheduleProjects()
        {
            var projects = await GetProjects();
            
            foreach (var project in projects)
            {
                var message = DeserializeProjectMessage(project);
                await _mediator.Send(message);
            }
        }
        
        private async Task ScheduleUsers()
        {
            var users = await GetUsers();

            foreach (var user in users)
            {
                var message = DeserializeUserMessage(user);
                await _mediator.Send(message);
            }
        }

        private async Task<List<int>> GetProjects()
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            return ctx.Projects.AsNoTracking().Select(x => x.Project_ID).Distinct().ToList();
        }

        private async Task<List<int>> GetUsers()
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
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