using Azure.Messaging.ServiceBus;
using CommunityToolkit.Diagnostics;
using Datahub.Core.Model.Context;
using Datahub.Functions.Extensions;
using Datahub.Infrastructure.Queues.Messages;
using Datahub.Infrastructure.Services;
using Datahub.Shared.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class ProjectInactiveHandler(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    QueuePongService pongService,
    ILoggerFactory loggerFactory)
{
    
    private readonly ILogger _logger = loggerFactory.CreateLogger<ProjectInactiveHandler>();

    [Function("ProjectInactiveHandler")]
    public async Task RunAsync(
        [ServiceBusTrigger(QueueConstants.ProjectInactiveQueueName, Connection = "DatahubServiceBus:ConnectionString")]
         ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("C# ServiceBus queue trigger started");
        
        if(await pongService.Pong(message.Body.ToString()))
        {
            return;
        }
        
        var output = await message.DeserializeAndUnwrapMessageAsync<ProjectInactiveMessage>();
        _logger.LogInformation($"C# ServiceBus queue trigger function processed message: {output}");
        
        Guard.IsNotNull(output, nameof(output));

        try
        {
            await HandleInactiveProject(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing project inactive message");
            throw;
        }
    }

    private async Task HandleInactiveProject(ProjectInactiveMessage output)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var project = await ctx.Projects
            .Include(p => p.Resources)
            .FirstAsync(p => p.Project_Acronym_CD == output.WorkspaceAcronym);
        
        project.Deleted_DT = DateTime.UtcNow;
        ctx.Projects.Update(project);
        await ctx.SaveChangesAsync();
        
        // TODO: mark each resource as to be deleted
        // TODO: send workspace definition to resource run queue
    }
}