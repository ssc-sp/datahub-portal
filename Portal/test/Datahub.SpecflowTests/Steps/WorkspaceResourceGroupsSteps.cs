using Datahub.Application.Configuration;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Extensions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps
{
    [Binding]
    public class WorkspaceResourceGroupsSteps(
        ScenarioContext scenarioContext,
        IWorkspaceResourceGroupsManagementService sut,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        [Given(@"a workspace with a new-project-template")]
        public async Task GivenAWorkspaceWithANewProjectTemplate()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            scenarioContext.Set(Testing.ResourceGroupName1, "expectedResourceGroup");
        }

        [When(@"the resource group is requested")]
        public async Task WhenTheResourceGroupIsRequested()
        {
            var acronym = scenarioContext.Get<string>("workspaceAcronym");
            try
            {
                var resourceGroups = await sut.GetWorkspaceResourceGroupsAsync(acronym);
                scenarioContext.Set(resourceGroups, "resourceGroups");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the result should be the expected resource group")]
        public void ThenTheResultShouldBeTheExpectedResourceGroup()
        {
            var resourceGroups = scenarioContext.Get<List<string>>("resourceGroups");
            var expectedResourceGroup = scenarioContext.Get<string>("expectedResourceGroup");

            resourceGroups.Should().Contain(expectedResourceGroup);
        }

        [Given(@"a workspace with an empty new-project-template and a blob-storage")]
        public async Task GivenAWorkspaceWithAnEmptyNewProjectTemplateAndABlobStorage()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            scenarioContext.Set(Testing.ResourceGroupName1, "expectedResourceGroup");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
            var newProjectTemplate = ctx.Project_Resources2.First(p =>
                p.ProjectId == project.Project_ID && p.ResourceType ==
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate));
            newProjectTemplate.JsonContent = "{}";
            ctx.Project_Resources2.Update(newProjectTemplate);
            await ctx.SaveChangesAsync();
        }

        [Given(@"a workspace with RG not stored in DB")]
        public async Task GivenAWorkspaceWithRgNotStoredInDb()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym2, "workspaceAcronym");
            scenarioContext.Set(Testing.ResourceGroupName2, "expectedResourceGroup");
        }

        [Given(@"an invalid workspace")]
        public void GivenAnInvalidWorkspace()
        {
            scenarioContext.Set(Testing.InvalidWorkspaceAcronym, "workspaceAcronym");
        }

        [Then(@"an exception should be thrown")]
        public void ThenAnExceptionShouldBeThrown()
        {
            var success = scenarioContext.Get<bool>("success");
            success.Should().BeFalse();
        }

        [Given(@"a subscription with resource groups")]
        public void GivenASubscriptionWithResourceGroups()
        {
            scenarioContext.Set(Testing.WorkspaceSubscriptionGuid, "subscriptionId");
            scenarioContext.Set(Testing.ResourceGroupName2, "expectedResourceGroup");
        }

        [When(@"the subscription resource groups are requested")]
        public async Task WhenTheSubscriptionResourceGroupsAreRequested()
        {
            var subId = scenarioContext.Get<string>("subscriptionId");
            try
            {
                var resourceGroups = await sut.GetAllSubscriptionResourceGroupsAsync(subId);
                scenarioContext.Set(resourceGroups, "resourceGroups");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Given(@"an invalid subscription")]
        public void GivenAnInvalidSubscription()
        {
            scenarioContext.Set(Testing.InvalidSubscriptionId, "subscriptionId");
        }

        [Given(@"a list of projects with resource groups")]
        public async Task GivenAListOfProjectsWithResourceGroups()
        {
            scenarioContext.Set(new List<string> { Testing.ResourceGroupName1, Testing.ResourceGroupName2 }, "expectedResourceGroups");
        }
        
        [When(@"all the resource groups are requested")]
        public async Task WhenAllTheResourceGroupsAreRequested()
        {
            try
            {
                var resourceGroups = await sut.GetAllResourceGroupsAsync();
                scenarioContext.Set(resourceGroups, "resourceGroups");
                scenarioContext.Set(true, "success");
            }
            catch
            {
                scenarioContext.Set(false, "success");
            }
        }

        [Then(@"the result should be the expected resource groups")]
        public void ThenTheResultShouldBeTheExpectedResourceGroups()
        {
            var resourceGroups = scenarioContext.Get<List<string>>("resourceGroups");
            var expectedResourceGroups = scenarioContext.Get<List<string>>("expectedResourceGroups");

            resourceGroups.Should().BeEquivalentTo(expectedResourceGroups);
        }

        [Given(@"a workspace with a new-project-template and a databricks")]
        public async Task GivenAWorkspaceWithANewProjectTemplateAndADatabricks()
        {
            scenarioContext.Set(Testing.WorkspaceAcronym, "workspaceAcronym");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.First(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
            var mockRg = $"fsdh_proj_{Testing.WorkspaceAcronym}_test_rg";
            var newProjectTemplate = ctx.Project_Resources2.First(p =>
                p.ProjectId == project.Project_ID && p.ResourceType ==
                TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate));
            newProjectTemplate.JsonContent = $"{{\"resource_group_name\": \"{mockRg}\"}}";
            ctx.Project_Resources2.Update(newProjectTemplate);
            var databricks = new Project_Resources2
            {
                ProjectId = project.Project_ID,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                JsonContent = "{}",
                ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks)
            };
            ctx.Project_Resources2.Add(databricks);
            await ctx.SaveChangesAsync();
            var dbkRg = project.GetDbkResourceGroupName(mockRg);
            scenarioContext.Set(new List<string> {mockRg, dbkRg}, "expectedResourceGroups");
        }
    }
}