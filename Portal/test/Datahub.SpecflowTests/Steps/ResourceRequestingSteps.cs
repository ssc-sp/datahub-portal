using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Services.Projects;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public sealed class ResourceRequestingSteps(
        ScenarioContext scenarioContext,
        IRequestManagementService requestManagementService,
        IResourceMessagingService resourceMessagingService,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {

        [Given(@"a workspace without a database resource")]
        public async Task GivenAWorkspaceWithoutADatabaseResource()
        {
            const string projectAcronym = "test";
            await GenerateWorkspaceHelper.GenerateWorkspace(dbContextFactory, projectAcronym);
            
            scenarioContext["projectAcronym"] = projectAcronym;
        }
        
        [Given(@"a workspace without a (.*) resource")]
        public async Task GivenAWorkspaceWithoutAResource(string resourceName)
        {
            var resourceType = TransformResourceName(resourceName);
            var projectAcronym = $"{resourceType}-acronym";
            await GenerateWorkspaceHelper.GenerateWorkspace(dbContextFactory, projectAcronym);

            scenarioContext[$"acronym:{resourceName}"] = projectAcronym;
        }

        [Given(@"a current user")]
        public async Task GivenACurrentUser()
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var currentUser = new PortalUser()
            {
                GraphGuid = Testing.CurrentUserGuid.ToString(),
                Email = Testing.CurrentUserEmail
            };
            
            ctx.PortalUsers.Add(currentUser);
            await ctx.SaveChangesAsync();
        }


        [When(@"a current user requests to create a workspace database")]
        public async Task WhenACurrentUserRequestsToCreateAWorkspaceDatabase()
        {
            var projectAcronym = scenarioContext["projectAcronym"] as string;
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            var currentUser = await ctx.PortalUsers
                .FirstOrDefaultAsync(u => u.GraphGuid == Testing.CurrentUserGuid.ToString());
            
            var terraformTemplate = new TerraformTemplate(TerraformTemplate.AzurePostgres, TerraformStatus.CreateRequested);
            
            await requestManagementService.HandleTerraformRequestServiceAsync(project, terraformTemplate, currentUser);
        }
        
        
        [When(@"a current user requests to run a (.*) for a workspace")]
        public async Task WhenACurrentUserRequestsToRunAForAWorkspace(string resourceName)
        {
            
            var resourceType = TransformResourceName(resourceName);
            var terraformTemplate = new TerraformTemplate(resourceType, TerraformStatus.CreateRequested);
            var projectAcronym = scenarioContext[$"acronym:{resourceName}"] as string;
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            var currentUser = await ctx.PortalUsers
                .FirstOrDefaultAsync(u => u.GraphGuid == Testing.CurrentUserGuid.ToString());
            
            await requestManagementService.HandleTerraformRequestServiceAsync(project, terraformTemplate, currentUser);
        }

        [Then(@"there should be a workspace database resource created")]
        public async Task ThenThereShouldBeAWorkspaceDatabaseResourceCreated()
        {
            var projectAcronym = scenarioContext["projectAcronym"] as string;
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            var databaseResource = project?.Resources
                .FirstOrDefault(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres));
            
            databaseResource.Should().NotBeNull();
            databaseResource?.JsonContent.Should().NotBeNullOrEmpty();
        }

        [Then(@"there should be a workspace (.*) resource created")]
        public async Task ThenThereShouldBeAWorkspaceResourceCreated(string resourceName)
        {
            var resourceType = TransformResourceName(resourceName);
            var projectAcronym = scenarioContext[$"acronym:{resourceName}"] as string;
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            var expectedResource = project?.Resources
                .FirstOrDefault(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(resourceType));
            
            expectedResource.Should().NotBeNull();
            expectedResource?.JsonContent.Should().NotBeNullOrEmpty();
            expectedResource?.RequestedAt.Should().BeBefore(DateTime.UtcNow);
            expectedResource?.RequestedById.Should().BeGreaterThan(0);
        }

        [Then(@"there should be a queue message to create a workspace database")]
        public void ThenThereShouldBeAQueueMessageToCreateAWorkspaceDatabase()
        {
            resourceMessagingService
                .Received()
                .SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
        }

        public static string TransformResourceName(string constantName) => constantName switch
        {
            nameof(TerraformTemplate.AzureAppService) => TerraformTemplate.AzureAppService,
            nameof(TerraformTemplate.AzureArcGis) => TerraformTemplate.AzureArcGis,
            nameof(TerraformTemplate.AzureDatabricks) => TerraformTemplate.AzureDatabricks,
            nameof(TerraformTemplate.AzurePostgres) => TerraformTemplate.AzurePostgres,
            nameof(TerraformTemplate.AzureStorageBlob) => TerraformTemplate.AzureStorageBlob,
            nameof(TerraformTemplate.AzureVirtualMachine) => TerraformTemplate.AzureVirtualMachine,
            nameof(TerraformTemplate.ContactUs) => TerraformTemplate.ContactUs,
            nameof(TerraformTemplate.NewProjectTemplate) => TerraformTemplate.NewProjectTemplate,
            nameof(TerraformTemplate.VariableUpdate) => TerraformTemplate.VariableUpdate,
            _ => throw new ArgumentOutOfRangeException(nameof(constantName), constantName, "Error transforming resource name with value: " + constantName)
        };

        [Then(@"there should be (.*) messages in resource messaging queue")]
        public void ThenThereShouldBeMessagesInResourceMessagingQueue(int numberOfQueueMessages)
        {
            resourceMessagingService
                .Received(numberOfQueueMessages)
                .SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
        }

        [Then(@"there should not be a workspace (.*) resource created")]
        public async Task ThenThereShouldNotBeAWorkspaceResourceCreated(string resourceName)
        {
            var resourceType = TransformResourceName(resourceName);
            var projectAcronym = scenarioContext[$"acronym:{resourceName}"] as string;
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .Include(p => p.Resources)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            var expectedResource = project?.Resources
                .FirstOrDefault(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(resourceType));
            
            expectedResource.Should().BeNull();
        }
    }
}