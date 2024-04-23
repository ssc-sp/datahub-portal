using Datahub.Core.Model.Subscriptions;

namespace Datahub.Application.Services;

public interface IDatahubAzureSubscriptionService
{
    /// <summary>
    /// Retrieves a list of Datahub Azure subscriptions from the database.
    /// </summary>
    /// <returns>The task result contains the list of Datahub Azure subscriptions.</returns>
    Task<List<DatahubAzureSubscription>> ListSubscriptionsAsync();

    /// <summary>
    /// Adds a Datahub Azure subscription to the database.
    /// </summary>
    /// <param name="subscription">The Datahub Azure subscription to be added.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddSubscriptionAsync(DatahubAzureSubscription subscription);

    /// <summary>
    /// Disables a Datahub Azure subscription.
    /// </summary>
    /// <param name="subscriptionId">The GUID of the Datahub Azure subscription to be disabled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisableSubscriptionAsync(string subscriptionId);
}