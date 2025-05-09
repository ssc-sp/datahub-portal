using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Services.CatalogSearch;
using Microsoft.EntityFrameworkCore;
using Datahub.Core.Extensions;
using Microsoft.Extensions.Logging;
using Datahub.Shared.Entities;
using Datahub.Shared;
using Google.Api.Gax.ResourceNames;
using Microsoft.Graph.Models.Search;
using Datahub.Core.Components;
using Datahub.Core.Model.Projects;

namespace Datahub.Infrastructure.Services
{
    public class ProjectDeletionService(
        IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        ILogger<WorkspaceCreationService> logger,
        IUserInformationService userInformationService,
        IResourceMessagingService resourceMessagingService
        ) : IProjectDeletionService
    {
        public async Task<bool> DeleteWorkspace(string acronym, Project_Delete_Questionnaire questionnaire)
        {
            try
            {
                await using var ctx = await datahubProjectDbFactory.CreateDbContextAsync();

                var resources = await ctx.Project_Resources2
                       .Include(r => r.Project)
                       .Where(r => r.Project.Project_Acronym_CD == acronym)
                       .ToListAsync(CancellationToken.None);

                var rgName = string.Empty;

                foreach (var resource in resources)
                {
                    if (questionnaire.Project is null)
                    {
                        questionnaire.Project = resource.Project;
                    }

                    resource.Status = resource.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate) ? TerraformStatus.DeleteRequested : TerraformStatus.Deleted;
                    resource.Project.Deleted_DT = resource.Project.Deleted_DT ?? DateTime.Now;
                    if (resource.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate))
                    {
                        rgName = resource.Project.GetResourceGroupName();
                    }
                    ctx.Project_Resources2.Update(resource);
                }

                
                
                var currentUser = await userInformationService.GetCurrentPortalUserAsync();
                questionnaire.DeletedDate = DateTime.Now;
                questionnaire.DeletedBy = currentUser;

                ctx.Attach(currentUser);
                ctx.Project_Delete_Questionnaires.Add(questionnaire);

                await ctx.SaveChangesAsync(CancellationToken.None);

                var workspaceDefinition = await resourceMessagingService.GetWorkspaceDefinition(acronym);
                workspaceDefinition.ResourceGroupName = rgName;
                workspaceDefinition.RequestingUserEmail = currentUser.Email;
                await resourceMessagingService.SendToTerraformQueue(workspaceDefinition);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting workspace - {acronym}");
                return false;
            }
        }

        public async Task<bool> CleanWorkspaceFromRecentLinks(string workspaceAcronym)
        {
            try
            {
                await using var ctx = await datahubProjectDbFactory.CreateDbContextAsync();

                var recentLinks = await ctx.UserRecentLinks
                    .Where(link => link.DataProject == workspaceAcronym)
                    .ToListAsync(CancellationToken.None);

                ctx.UserRecentLinks.RemoveRange(recentLinks);
                await ctx.SaveChangesAsync(CancellationToken.None);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting workspace from recent links - {workspaceAcronym}");
                return false;
            }
        }

        public async Task<bool> CleanResourceFromRecentLinks(string section, string workspaceAcronym)
        {
            try
            {
                await using var ctx = await datahubProjectDbFactory.CreateDbContextAsync();

                DatahubLinkType linkType = section switch
                {
                    //currently can only delete postgres and app service, and postgres doesnt get added to recent links yet
                    TerraformTemplate.AzureAppService => DatahubLinkType.AzureWebApp,
                    _ => DatahubLinkType.Undefined
                };

                if (linkType != DatahubLinkType.Undefined)
                { 
                    var recentLinks = await ctx.UserRecentLinks
                        .Where(link => link.LinkType == linkType && link.DataProject == workspaceAcronym)
                        .ToListAsync(CancellationToken.None);

                    ctx.UserRecentLinks.RemoveRange(recentLinks);
                    await ctx.SaveChangesAsync(CancellationToken.None);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting recent links for - {section} - for workspace - {workspaceAcronym}");
                return false;
            }            
        }
    }
}
