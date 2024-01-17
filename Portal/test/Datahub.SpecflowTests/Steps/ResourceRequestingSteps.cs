using Datahub.Application.Services;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services.Projects;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

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
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            const string projectAcronym = "test";
            var project = new Datahub_Project()
            {
                Project_Acronym_CD = projectAcronym,
            };
            
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
            
            scenarioContext["projectAcronym"] = projectAcronym;
        }
        
        
        [Given(@"a workspace without a (.*) resource")]
        public async Task GivenAWorkspaceWithoutAResource(string resourceName)
        {
            var resourceType = TransformResourceName(resourceName);
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var projectAcronym = $"{resourceType}-acronym";
            var project = new Datahub_Project()
            {
                Project_Acronym_CD = projectAcronym,
            };
            
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
            
            scenarioContext[$"acronym:{resourceName}"] = projectAcronym;
        }
        
        [Given(@"a current user")]
        public async Task GivenACurrentUser()
        {
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var currentUser = new PortalUser()
            {
                GraphGuid = Testing.CURRENT_USER_GUID.ToString(),
                Email = Testing.CURRENT_USER_EMAIL
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
                .FirstOrDefaultAsync(u => u.GraphGuid == Testing.CURRENT_USER_GUID.ToString());
            
            await requestManagementService.HandleTerraformRequestServiceAsync(project, TerraformTemplate.AzurePostgres, currentUser);
        }
        
        
        [When(@"a current user requests to create a workspace (.*)")]
        public async Task WhenACurrentUserRequestsToCreateAWorkspace(string resourceName)
        {
            var resourceType = TransformResourceName(resourceName);
            var projectAcronym = scenarioContext[$"acronym:{resourceName}"] as string;
            
            await using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == projectAcronym);
            
            var currentUser = await ctx.PortalUsers
                .FirstOrDefaultAsync(u => u.GraphGuid == Testing.CURRENT_USER_GUID.ToString());
            
            await requestManagementService.HandleTerraformRequestServiceAsync(project, resourceType, currentUser);
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

        private string TransformResourceName(string constantName) => constantName switch
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
            nameof(TerraformTemplate.Default) => TerraformTemplate.Default.Name,
            _ => throw new ArgumentOutOfRangeException(nameof(constantName), constantName, null)
        };

        [Then(@"there should be (.*) queue message to create provision the resource\(s\)")]
        public void ThenThereShouldBeQueueMessageToCreateProvisionTheResourceS(int numberOfQueueMessages)
        {
            resourceMessagingService
                .Received(numberOfQueueMessages)
                .SendToTerraformQueue(Arg.Any<WorkspaceDefinition>());
        }
    }
}