using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public sealed class DatahubAzureSubscriptionSteps(
    DatahubPortalConfiguration portalConfiguration,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IDatahubAzureSubscriptionService datahubAzureSubscriptionService,
    ScenarioContext scenarioContext)
{
    [Given(@"a datahub azure subscription service")]
    public void GivenADatahubAzureSubscriptionService()
    {
        datahubAzureSubscriptionService.Should().NotBeNull();
    }

    [Given(@"at least one subscription exists")]
    public async Task GivenAtLeastOneSubscriptionExists()
    {
        var subscription = new DatahubAzureSubscription()
        {
            SubscriptionId = Testing.WORKSPACE_SUBSCRIPTION_GUID,
            TenantId = Testing.WORKSPACE_TENANT_GUID
        };

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.AzureSubscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
    }

    [When(@"the list of subscriptions is requested")]
    public void WhenTheListOfSubscriptionsIsRequested()
    {
        var subscriptions = datahubAzureSubscriptionService.ListSubscriptionsAsync().Result;
        scenarioContext["subscriptions"] = subscriptions;
    }

    [Then(@"the list of subscriptions is returned")]
    public void ThenTheListOfSubscriptionsIsReturned()
    {
        var subscriptions = scenarioContext["subscriptions"] as List<DatahubAzureSubscription>;
        subscriptions.Should().NotBeNull();
    }

    [Then(@"the list of subscriptions contains at least one subscription")]
    public void ThenTheListOfSubscriptionsContainsAtLeastOneSubscription()
    {
        var subscriptions = scenarioContext["subscriptions"] as List<DatahubAzureSubscription>;
        subscriptions.Should().NotBeEmpty();
        subscriptions!.Count.Should().BeGreaterThan(0);
        
    }
}