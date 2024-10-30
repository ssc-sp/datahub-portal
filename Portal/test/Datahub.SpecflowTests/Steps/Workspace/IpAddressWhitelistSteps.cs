using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Portal.Pages.Workspace.Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps.Workspace;

[Binding]
public class IpAddressWhitelistSteps(ScenarioContext scenarioContext)
{
    [Given(@"a workspace and an azure subscription id for an DatabaseIpWhitelistTable component")]
    public async Task GivenAWorkspaceAndAnAzureSubscriptionIdForAnDatabaseIpWhitelistTableComponent()
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContextFactory = new SpecFlowDbContextFactory(options);

        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var azureSubscription = new DatahubAzureSubscription
        {
            SubscriptionId = Testing.WorkspaceSubscriptionGuid,
            SubscriptionName = Testing.SubscriptionName,
            TenantId = Testing.WorkspaceTenantGuid,
        };

        var workspace = new Datahub_Project
        {
            Project_Name = Testing.WorkspaceName,
            Project_Acronym_CD = Testing.WorkspaceAcronym,
            DatahubAzureSubscription = azureSubscription
        };

        dbContext.AzureSubscriptions.Add(azureSubscription);
        dbContext.Projects.Add(workspace);

        await dbContext.SaveChangesAsync();
        
        scenarioContext["dbContextFactory"] = dbContextFactory;
    }

    [When(@"the workspace subscription id is retrieved")]
    public async Task WhenTheWorkspaceSubscriptionIdIsRetrieved()
    {
        var dbContextFactory = scenarioContext["dbContextFactory"] as IDbContextFactory<DatahubProjectDBContext>;
        await using var dbContext = await dbContextFactory!.CreateDbContextAsync();
        
        var workspaceSubscriptionId = await DatabaseIpWhitelistTable.RetrieveWorkspaceSubscriptionId(Testing.WorkspaceAcronym, dbContext);
        scenarioContext["WorkspaceSubscriptionId"] = workspaceSubscriptionId;
    }

    [Then(@"the workspace subscription id should be the same as the azure subscription id")]
    public void ThenTheWorkspaceSubscriptionIdShouldBeTheSameAsTheAzureSubscriptionId()
    {
        var workspaceSubscriptionId = scenarioContext["WorkspaceSubscriptionId"] as string;
        workspaceSubscriptionId.Should().Be(Testing.WorkspaceSubscriptionGuid);
    }
}