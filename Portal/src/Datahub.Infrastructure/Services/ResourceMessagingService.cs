using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Extensions;
using Datahub.Shared;
using Datahub.Shared.Configuration;
using Datahub.Shared.Entities;
using Datahub.Shared.Exceptions;
using Datahub.Core.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Datahub.Core.Configuration;

namespace Datahub.Infrastructure.Services;

public class ResourceMessagingService(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    ISendEndpointProvider sendEndpointProvider,
    IServiceBusConfiguration messageBusConfiguration,
    IWorkspaceVersionService workspaceVersionService,
    ISubnetPoolService subnetPoolService)
    : IResourceMessagingService
{
    public async Task SendToTerraformQueue(WorkspaceDefinition workspaceDefinition)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.ResourceRunRequestQueueName, workspaceDefinition); 
    }

   

    public async Task QueueRBACSync(WorkspaceDefinition workspaceDefinition)
    {
        await sendEndpointProvider.SendDatahubServiceBusMessage(QueueConstants.UserRunRequestQueueName, workspaceDefinition); 
    }

    public async Task<WorkspaceDefinition> CreateWorkspaceDefinition(string projectAcronym, string requestingUserEmail = "system-generated", string? cbrId = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.UserRoles)
            .ThenInclude(u => u.PortalUser)
            .ThenInclude(u => u.EntraUser)
            .Include(p => p.Resources)
            .Include(p => p.DatahubAzureSubscription)
            .Include(p => p.ParentGCHostingBudget)
            .AsSingleQuery()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);

        if (project is null)
        {
            throw new ProjectNotFoundException($"Project {projectAcronym} not found.");
        }
        
        var entraUsers = project.UserRoles
            .Where(u => u.PortalUser != null && u.PortalUser.EntraUser != null)
            .Select(u => new TerraformUser
            {
                ObjectId = u.PortalUser!.EntraUser!.GraphGuid, 
                Email = u.PortalUser!.Email ?? throw new InvalidOperationException($"User {u.PortalUser!.EntraUser!.GraphGuid} email is missing"), 
                Role = RequestManagementService.GetTerraformUserRole(u)
            })
            .ToList();

        var tfWorkspace = project.ToResourceWorkspace(entraUsers, messageBusConfiguration);
        var templates = project.Resources
            .Where(r => r.ResourceType != TerraformTemplate.VariableUpdate && r.Status != TerraformStatus.Deleted)
            .Select(r => r.ToTerraformTemplate())
            .ToList();



        tfWorkspace.Version = tfWorkspace.Version == "latest" ? await workspaceVersionService.GetLatestVersionAsync() : tfWorkspace.Version;

        var appData = new WorkspaceAppData
        {
            DatabricksHostUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project, null),
            AppServiceConfiguration = TerraformVariableExtraction.ExtractAppServiceConfiguration(project),
            PostgresConfiguration = TerraformVariableExtraction.ExtractPostgresConfiguration(project),
            DatabricksConfiguration = TerraformVariableExtraction.ExtractDatabricksConfiguration(project)
        };

        // For Protected B workspaces that include an App Service, assign (or retrieve the
        // already-assigned) subnet from the VNet pool and inject it into the app configuration.
        if (tfWorkspace.IsProtectedB
            && templates.Any(t => t.Name == TerraformTemplate.AzureAppService)
            && appData.AppServiceConfiguration is not null)
        {
            appData.AppServiceConfiguration.SubnetId =
                await subnetPoolService.ClaimOrGetAppServiceSubnetIdAsync(
                    project.Project_ID,
                    tfWorkspace.SubscriptionId);
        }

        return new WorkspaceDefinition
        {
            Workspace = tfWorkspace,
            Templates = templates,
            AppData = appData,
            RequestingUserEmail = requestingUserEmail,
            ResourceGroupName = project.GetResourceGroupName(),
            CBRID = project.ParentGCHostingBudget?.CBRID ?? throw new InvalidOperationException($"Project {projectAcronym} is missing a CBRID in its parent GC Hosting Budget."),
        };
    }

    
}
