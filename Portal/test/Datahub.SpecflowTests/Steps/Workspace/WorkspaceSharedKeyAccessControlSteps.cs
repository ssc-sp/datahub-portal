using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Portal.Pages.Workspace.Settings;
using Datahub.Shared.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;
using Reqnroll.BoDi;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class WorkspaceSharedKeyAccessControlSteps(
    ScenarioContext scenarioContext,
     IObjectContainer objectContainer,
    DatahubPortalConfiguration datahubPortalConfiguration,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
{
    [Given(@"a workspace in (.*) with (.*) and a storage resource named (.*) and resource group named (.*)")]
    public async Task GivenAWorkspaceInWithAndAStorageResourceNamed(string subscriptionId, string acronym, string storageAccountName, string resourceGroupName)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var workspace = new Datahub_Project()
        {
            Project_Acronym_CD = acronym,
            Resources = new List<Project_Resources2>()
        };
        
        var azureSubscription = new DatahubAzureSubscription
        {
            SubscriptionId = subscriptionId,
            TenantId = Testing.WorkspaceTenantGuid,
            SubscriptionName = Testing.SubscriptionName
        };
        
        var storageResource = new Project_Resources2
        {
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob),
            JsonContent = $"{{\"storage_account\": \"{storageAccountName}\"}}",
        };
        
        var resourceGroup = new Project_Resources2
        {
            ResourceType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate),
            JsonContent = $"{{\"resource_group_name\": \"{resourceGroupName}\"}}",
        };
        
        workspace.Resources.Add(storageResource);
        workspace.Resources.Add(resourceGroup);
        workspace.DatahubAzureSubscription = azureSubscription;
        
        dbContext.Projects.Add(workspace);
        dbContext.AzureSubscriptions.Add(azureSubscription);
        dbContext.Project_Resources2.Add(storageResource);
        
        await dbContext.SaveChangesAsync();
    }

    [When(@"I load the storage resource id for acronym (.*)")]
    public async Task WhenILoadTheStorageResourceIdForAcronym(string acronym)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var result = await WorkspaceSharedKeyAccessControl.LoadStorageResourceId(dbContext, acronym);
        scenarioContext.Set(result, "storageResourceId");
    }

    [Then(@"the result should equal (.*)")]
    public void ThenTheResultShouldEqual(string p0)
    {
        var result = scenarioContext.Get<string>("storageResourceId");
        result.Should().Be(p0);
    }
}