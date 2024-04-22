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
}