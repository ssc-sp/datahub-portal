using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services.Projects;
using Datahub.ProjectTools.Services;
using Datahub.Shared.Entities;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using Datahub.Infrastructure.Services;

namespace Datahub.Functions
{
    public class ProjectInactivityUpdater
    {
        private readonly ILogger<ProjectInactivityUpdater> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly IMediator _mediator;
        private readonly QueuePongService _pongService;
        private readonly ProjectInactivityService _inactivityService;

        public ProjectInactivityUpdater(ILoggerFactory loggerFactory,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IMediator mediator,
            QueuePongService pongService, ProjectInactivityService inactivityService)
        {
            _logger = loggerFactory.CreateLogger<ProjectInactivityUpdater>();
            _dbContextFactory = dbContextFactory;
            _mediator = mediator;
            _pongService = pongService;
            _inactivityService = inactivityService;
        }

        [Function("ProjectInactivityUpdater")]
        public async Task Run(
            [QueueTrigger("%QueueProjectInactivityUpdate%", Connection = "DatahubStorageConnectionString")]
            string queueItem,
            CancellationToken cancellationToken)
        {
            // test for ping
            if (await _pongService.Pong(queueItem))
                return;

            // deserialize message
            var message = DeserializeQueueMessage(queueItem);
            
            // update last time user logged in the project
            var lastLogin = await _inactivityService.UpdateDaysSinceLastLogin(message.ProjectId, cancellationToken);
            
            // get project whitelisted status
            var whitelisted = await _inactivityService.GetProjectWhitelisted(message.ProjectId, cancellationToken);
            
            // get project retirement date
            var retirementDate = await _inactivityService.GetProjectRetirementDate(message.ProjectId, cancellationToken);
            
            // log information
            _logger.LogInformation("Project {ProjectId} has last login {LastLogin}, has whitelisted {Whitelisted} with retirement date {RetirementDate}", 
                message.ProjectId, lastLogin, whitelisted, retirementDate);
            
            // send to mediator
            await _mediator.Send(new ProjectInactivityNotificationMessage(message.ProjectId, lastLogin, whitelisted, retirementDate), cancellationToken);
        }

        private ProjectInactivityUpdateMessage DeserializeQueueMessage(string queueItem)
        {
            var message = JsonSerializer.Deserialize<ProjectInactivityUpdateMessage>(queueItem);

            // verify message
            if (message is null || message.ProjectId <= 0)
            {
                throw new Exception($"Invalid queue message:\n{queueItem}");
            }

            return message;
        }
    }
}