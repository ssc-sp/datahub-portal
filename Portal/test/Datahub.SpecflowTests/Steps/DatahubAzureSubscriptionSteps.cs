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

    [When(@"a new subscription is added")]
    public async Task WhenANewSubscriptionIsAdded()
    {
        var subscription = new DatahubAzureSubscription()
        {
            SubscriptionId = portalConfiguration.AzureAd.SubscriptionId,
            TenantId = portalConfiguration.AzureAd.TenantId
        };

        await datahubAzureSubscriptionService.AddSubscriptionAsync(subscription);
    }

    [Then(@"the subscription is added to the list of subscriptions")]
    public async Task ThenTheSubscriptionIsAddedToTheListOfSubscriptions()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var subscriptions = await ctx.AzureSubscriptions.ToListAsync();
        subscriptions.Should().NotBeNull();
        subscriptions!.Count.Should().BeGreaterThan(0);
        subscriptions!.Any(s => s.SubscriptionId == portalConfiguration.AzureAd.SubscriptionId).Should().BeTrue();
    }

    [When(@"an invalid subscription is added")]
    public async Task WhenAnInvalidSubscriptionIsAdded()
    {
        try
        {
            var subscription = new DatahubAzureSubscription()
            {
                SubscriptionId = "invalid-subscription-id",
                TenantId = "invalid-tenant-id"
            };

            await datahubAzureSubscriptionService.AddSubscriptionAsync(subscription);
        }
        catch (Exception e)
        {
            scenarioContext["exception"] = e;
        }
    }

    [Then(@"an error is returned")]
    public void ThenAnErrorIsReturned()
    {
        var exception = scenarioContext["exception"] as Exception;
        exception.Should().NotBeNull();
    }

    [When(@"a subscription is deleted")]
    public async Task WhenASubscriptionIsDeleted()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var subscription = await ctx.AzureSubscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == Testing.WORKSPACE_SUBSCRIPTION_GUID);

        subscription.Should().NotBeNull();
        await datahubAzureSubscriptionService.DisableSubscriptionAsync(subscription!);
    }

    [Given(@"at exactly one subscription exists")]
    public async Task GivenAtExactlyOneSubscriptionExists()
    {
        var subscription = new DatahubAzureSubscription()
        {
            SubscriptionId = Testing.WORKSPACE_SUBSCRIPTION_GUID,
            TenantId = Testing.WORKSPACE_TENANT_GUID
        };

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.AzureSubscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
        
        var subscriptions = await ctx.AzureSubscriptions.ToListAsync();
        subscriptions.Should().NotBeNull();
        subscriptions!.Count.Should().Be(1);
    }

    [Then(@"there should be no subscriptions")]
    public async Task ThenThereShouldBeNoSubscriptions()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var subscriptions = await ctx.AzureSubscriptions.ToListAsync();
        subscriptions.Should().NotBeNull();
        subscriptions!.Count.Should().Be(0);
    }
}