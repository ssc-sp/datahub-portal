using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Subscriptions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Subscriptions;

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
        return await ctx.AzureSubscriptions
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a Datahub Azure subscription with the specified subscriptionId exists.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to check.</param>
    /// <returns>Returns true if the subscription exists, otherwise false.</returns>
    public async Task<DatahubAzureSubscription> SubscriptionExistsAsync(string subscriptionId)
    {
        var subscriptionResource = await FetchSubscriptionResource(subscriptionId);
        return new DatahubAzureSubscription
        {
            TenantId = subscriptionResource?.TenantId,
            SubscriptionId = subscriptionId,
            SubscriptionName = subscriptionResource?.SubscriptionName
        };
    }

    /// <summary>
    /// Adds a new Datahub Azure subscription to the database.
    /// </summary>
    /// <param name="subscription">The Datahub Azure subscription to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the subscription is not found or there are access permission issues.</exception>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddSubscriptionAsync(DatahubAzureSubscription subscription)
    {
        var subscriptionResource = await FetchSubscriptionResource(subscription.SubscriptionId);

        subscription.SubscriptionName = subscriptionResource.SubscriptionName;
        subscription.TenantId = subscriptionResource.TenantId;

        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.AzureSubscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves the subscription resource for a given subscription ID.
    /// </summary>
    /// <param name="subscriptionId">The GUID of the subscription.</param>
    /// <returns>Returns a nullable response containing the SubscriptionResource if found; otherwise null.</returns>
    public virtual async Task<DatahubAzureSubscription> FetchSubscriptionResource(string subscriptionId)
    {
        // verify access
        var armClient = new ArmClient(new ClientSecretCredential(
            portalConfiguration.AzureAd.TenantId,
            portalConfiguration.AzureAd.InfraClientId,
            portalConfiguration.AzureAd.InfraClientSecret));

        try
        {
            var subscriptionResource = await armClient
                .GetSubscriptions()
                .GetIfExistsAsync(subscriptionId);
            
            return new DatahubAzureSubscription
            {
                TenantId = subscriptionResource.Value!.Data.TenantId.ToString(),
                SubscriptionId = subscriptionId,
                SubscriptionName = subscriptionResource.Value.Data.DisplayName
            };
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                "Subscription not found. Please check the subscription id and access permissions.", e);
        }
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

        if (workspacesUsingSubscription.Count != 0)
        {
            throw new InvalidOperationException("Subscription is in use by one or more workspaces.");
        }

        var subscription = await ctx.AzureSubscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

        if (subscription == null)
        {
            throw new InvalidOperationException("Subscription not found.");
        }

        ctx.AzureSubscriptions.Remove(subscription);
        await ctx.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves the number of remaining workspaces in a Datahub Azure subscription.
    /// </summary>
    /// <param name="subscriptionId">The GUID of the Datahub Azure subscription.</param>
    /// <returns>The task result contains the number of remaining workspaces.</returns>
    public async Task<int> NumberOfRemainingWorkspacesAsync(string subscriptionId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var workspacesUsingSubscription = await ctx.Projects
            .AsNoTracking()
            .Include(p => p.DatahubAzureSubscription)
            .Where(p => p.DatahubAzureSubscription.SubscriptionId == subscriptionId)
            .ToListAsync();

        var subscriptionStrategy = new DatahubSubscriptionStrategy(portalConfiguration);
        return subscriptionStrategy.NumberOfWorkspacesRemaining(workspacesUsingSubscription);

        //return IDatahubAzureSubscriptionService.MaxNumberOfWorkspaces - workspacesUsingSubscription.Count;
    }

    /// <summary>
    /// Retrieves the next available DatahubAzureSubscription from the database.
    /// </summary>
    /// <returns>The task result contains the next available DatahubAzureSubscription.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no subscriptions available or no subscriptions available with remaining workspaces.</exception>
    public async Task<DatahubAzureSubscription> NextSubscriptionAsync()
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var subscriptions = await ctx.AzureSubscriptions
            .AsNoTracking()
            .Include(e => e.Workspaces)
            .ToListAsync();

        if (subscriptions.Count == 0)
        {
            throw new InvalidOperationException("No subscriptions available.");
        }

        var subscriptionStrategy = new DatahubSubscriptionStrategy(portalConfiguration);
        return subscriptionStrategy.NextSubscription(subscriptions);
    }

    /// <summary>
    /// Updates a Datahub Azure subscription in the database.
    /// </summary>
    /// <param name="subscription">The Datahub Azure subscription to update.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    public async Task UpdateSubscriptionAsync(DatahubAzureSubscription subscription)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.AzureSubscriptions.Update(subscription);
        await ctx.SaveChangesAsync();
    }
}