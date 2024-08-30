using Datahub.Application.Services;
using Datahub.Application.Services.Subscriptions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public sealed class WorkspaceSubscriptionTargetingSteps(
    IResourceMessagingService resourceMessagingService,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IProjectCreationService projectCreationService,
    ScenarioContext scenarioContext)
{
    [Given(@"a workspace that has a subscription id")]
    public async Task GivenAWorkspaceThatHasASubscriptionId()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = new Datahub_Project
        {
            Project_Acronym_CD = Testing.WorkspaceAcronym,
        };
        
        var datahubAzureSubscription = new DatahubAzureSubscription()
        {
            SubscriptionId = Testing.WorkspaceSubscriptionGuid,
            TenantId = Testing.WorkspaceTenantGuid,
            SubscriptionName = Testing.SubscriptionName
        };
        
        workspace.DatahubAzureSubscription = datahubAzureSubscription;
        
        ctx.Projects.Add(workspace);
        await ctx.SaveChangesAsync();
    }




    [Then(@"the subscription id is included in the workspace definition")]
    public void ThenTheSubscriptionIdIsIncludedInTheWorkspaceDefinition()
    {
        var workspaceDefinition = scenarioContext["workspaceDefinition"] as WorkspaceDefinition;
        workspaceDefinition!.Workspace.SubscriptionId.Should().Be(Testing.WorkspaceSubscriptionGuid);
    }

    [Given(@"a new workspace is created")]
    public async Task GivenANewWorkspaceIsCreated()
    {
        await projectCreationService.CreateProjectAsync(Testing.WorkspaceName, Testing.WorkspaceAcronym, "Unspecified");
    }

    [Given(@"the next available subscription id is ""(.*)""")]
    public async Task GivenTheNextAvailableSubscriptionIdIs(string p0)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var subscriptions = await ctx.AzureSubscriptions.ToListAsync();
        subscriptions.Should().NotBeNull();
        subscriptions.Should().BeEmpty();
        
        var subscription = new DatahubAzureSubscription()
        {
            SubscriptionId = p0,
            TenantId = Testing.WorkspaceTenantGuid
        };
        
        ctx.AzureSubscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
    }

    [When(@"a two workspaces are created")]
    public async Task WhenATwoWorkspacesAreCreated()
    {
        await projectCreationService.CreateProjectAsync(Testing.WorkspaceName, Testing.WorkspaceAcronym, "Unspecified");
        await projectCreationService.CreateProjectAsync(Testing.WorkspaceName2, Testing.WorkspaceAcronym2, "Unspecified");
    }

    [Then(@"they should have different subscriptions")]
    public async Task ThenTheyShouldHaveDifferentSubscriptions()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        
        var workspace1 = await ctx.Projects
            .AsNoTracking()
            .Include(datahubProject => datahubProject.DatahubAzureSubscription!)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym);
        
        var workspace2 = await ctx.Projects
            .AsNoTracking()
            .Include(datahubProject => datahubProject.DatahubAzureSubscription!)
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == Testing.WorkspaceAcronym2);
        
        workspace1.Should().NotBeNull();
        workspace1!.DatahubAzureSubscription!.Should().NotBeNull();
        
        workspace2.Should().NotBeNull();
        workspace2!.DatahubAzureSubscription!.Should().NotBeNull();
        
        workspace1!.DatahubAzureSubscription!.SubscriptionId.Should().NotBe(workspace2!.DatahubAzureSubscription!.SubscriptionId);
    }
}