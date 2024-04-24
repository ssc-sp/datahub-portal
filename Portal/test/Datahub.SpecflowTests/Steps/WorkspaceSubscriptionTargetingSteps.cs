using Datahub.Application.Services;
using Datahub.Application.Services.Subscriptions;
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
    IDatahubAzureSubscriptionService datahubAzureSubscriptionService,
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
            Project_Acronym_CD = Testing.WORKSPACE_ACRONYM,
        };
        
        var datahubAzureSubscription = new DatahubAzureSubscription()
        {
            SubscriptionId = Testing.WORKSPACE_SUBSCRIPTION_GUID,
            TenantId = Testing.WORKSPACE_TENANT_GUID
        };
        
        workspace.DatahubAzureSubscription = datahubAzureSubscription;
        
        ctx.Projects.Add(workspace);
        await ctx.SaveChangesAsync();
    }

    [When(@"the workspace definition is requested")]
    public async Task WhenTheWorkspaceDefinitionIsRequested()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspace = await ctx.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Project_Acronym_CD == Testing.WORKSPACE_ACRONYM);
        
        var workspaceDefinition =  await resourceMessagingService.GetWorkspaceDefinition(workspace!.Project_Acronym_CD, string.Empty);
        scenarioContext["workspaceDefinition"] = workspaceDefinition;
    }


    [Then(@"the subscription id is included in the workspace definition")]
    public void ThenTheSubscriptionIdIsIncludedInTheWorkspaceDefinition()
    {
        var workspaceDefinition = scenarioContext["workspaceDefinition"] as WorkspaceDefinition;
        workspaceDefinition!.Workspace.SubscriptionId.Should().Be(Testing.WORKSPACE_SUBSCRIPTION_GUID);
    }

    [Given(@"a new workspace is created")]
    public async Task GivenANewWorkspaceIsCreated()
    {
        await projectCreationService.CreateProjectAsync(Testing.WORKSPACE_NAME, Testing.WORKSPACE_ACRONYM, "Unspecified");
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
            TenantId = Testing.WORKSPACE_TENANT_GUID
        };
        
        ctx.AzureSubscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
    }
}