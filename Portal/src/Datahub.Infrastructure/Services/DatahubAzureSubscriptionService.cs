using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

public class DatahubAzureSubscriptionService(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    DatahubPortalConfiguration portalConfiguration)
    : IDatahubAzureSubscriptionService
{
    /// <summary>
    /// Retrieves a list of Datahub Azure subscriptions from the database.
    /// </summary>
    /// <returns>The task result contains the list of Datahub Azure subscriptions.</returns>
    public async Task<List<DatahubAzureSubscription>> ListSubscriptionsAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.AzureSubscriptions.ToListAsync();
    }

    /// <summary>
    /// Adds a new Datahub Azure subscription to the database.
    /// </summary>
    /// <param name="subscription">The Datahub Azure subscription to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the subscription is not found or there are access permission issues.</exception>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddSubscriptionAsync(DatahubAzureSubscription subscription)
    {
        // verify access
        var armClient = new ArmClient(new ClientSecretCredential(
            portalConfiguration.AzureAd.TenantId,
            portalConfiguration.AzureAd.InfraClientId,
            portalConfiguration.AzureAd.InfraClientSecret));
        
        var subscriptionResource = await armClient
            .GetSubscriptions()
            .GetIfExistsAsync(subscription.SubscriptionId);
        
        if(subscriptionResource == null)
        {
            throw new InvalidOperationException("Subscription not found. Please check the subscription id and access permissions.");
        }
        
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.AzureSubscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
    }


    /// <summary>
    /// Disables a Datahub Azure subscription.
    ///
    /// TODO: Flag the subscription as disabled instead of removing it from the database.
    /// </summary>
    /// <param name="subscriptionId">The ID of the Datahub Azure subscription to be disabled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisableSubscriptionAsync(string subscriptionId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        var workspacesUsingSubscription = await ctx.Projects
            .Include(p => p.DatahubAzureSubscription)
            .Where(p => p.DatahubAzureSubscription.SubscriptionId == subscriptionId)
            .ToListAsync();
        
        if(workspacesUsingSubscription.Count != 0)
        {
            throw new InvalidOperationException("Subscription is in use by one or more workspaces.");
        }
        
        var subscription = await ctx.AzureSubscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);
        
        if(subscription == null)
        {
            throw new InvalidOperationException("Subscription not found.");
        }
        
        ctx.AzureSubscriptions.Remove(subscription);
        await ctx.SaveChangesAsync();
    }
}