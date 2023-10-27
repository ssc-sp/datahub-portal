using System;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Queues.Messages;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions
{
    public class ProjectInactivityScheduler
    {
        
        private readonly ILogger<ProjectInactivityScheduler> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IMediator _mediator;

        public ProjectInactivityScheduler(ILoggerFactory loggerFactory, IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IMediator mediator)
        {
            _logger = loggerFactory.CreateLogger<ProjectInactivityScheduler>();
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
        }
        
        [Function("ProjectInactivityScheduler")]
        public async Task Run([TimerTrigger("%ProjectInactivityCRON%")] TimerInfo timerInfo)
        {
            await RunScheduler();

        }
        
#if DEBUG
        [Function("ProjectInactivitySchedulerHttp")]
        public async Task RunHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
        {
            await RunScheduler();
        }
#endif

        private async Task RunScheduler()
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var projects = GetProjects(ctx);
            
            foreach (var project in projects)
            {
                var message = DeserializeMessage(project);
                await _mediator.Send(message);
            }
        }

        private List<int> GetProjects(DatahubProjectDBContext ctx)
        {
            return ctx.Projects.Select(x => x.Project_ID).Distinct().ToList();
        }

        private ProjectInactivityUpdateMessage DeserializeMessage(int projectId)
        {
            return new ProjectInactivityUpdateMessage(projectId);
        }
    }
    
}