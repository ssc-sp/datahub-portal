using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;



namespace Datahub.SpecflowTests.Steps;

[Binding]
public class WorkspaceDefinitionSteps(
    ScenarioContext scenarioContext,
    IResourceMessagingService resourceMessagingService,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
{
    [Then(@"the new-project-template resource should be in the created status")]
    public async Task ThenTheNewProjectTemplateResourceShouldBeInTheExistsStatus()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var project = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.Resources)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        
        var resource = project!.Resources.FirstOrDefault(r => r.ResourceType.EndsWith(TerraformTemplate.NewProjectTemplate));
        
        resource!.Status.Should().Be(TerraformStatus.Completed);
    }

    [Given(@"a workspace with a new-project-template resource that exists")]
    public async Task GivenAWorkspaceWithANewProjectTemplateResource()
    {
        await GenerateWorkspaceHelper.GenerateWorkspace(
            dbContextFactory, 
            Testing.WorkspaceAcronym, 
            TerraformTemplate.NewProjectTemplate, 
            TerraformStatus.Completed);
    }
    
    [When(@"the workspace definition is requested")]
    public async Task WhenTheWorkspaceDefinitionIsRequested()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = await ctx.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        
        var workspaceDefinition =  await resourceMessagingService.GetWorkspaceDefinition(workspace!.Project_Acronym_CD, string.Empty);
        scenarioContext["workspaceDefinition"] = workspaceDefinition;
    }

    [Then(@"the workspace definition should have a status of created for new-project-template")]
    public void ThenTheWorkspaceDefinitionShouldHaveAStatusOfCreatedForNewProjectTemplate()
    {
        var workspaceDefinition = scenarioContext["workspaceDefinition"] as WorkspaceDefinition;
        workspaceDefinition!.Templates
            .Any(t => 
                t.Name.Equals(TerraformTemplate.NewProjectTemplate) 
                && t.Status == TerraformStatus.Completed)
            .Should()
            .BeTrue();
    }
}