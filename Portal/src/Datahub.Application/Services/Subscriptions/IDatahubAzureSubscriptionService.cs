using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;

namespace Datahub.Application.Services.Subscriptions;

public interface IDatahubAzureSubscriptionService
{
    const int MaxNumberOfWorkspaces = 100;

    /// <summary>
    /// Retrieves a list of Datahub Azure subscriptions from the database.
    /// </summary>
    /// <returns>The task result contains the list of Datahub Azure subscriptions.</returns>
    Task<List<DatahubAzureSubscription>> ListSubscriptionsAsync();

    /// <summary>
    /// Checks if a Datahub Azure subscription with the specified subscriptionId exists.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to check.</param>
    /// <returns>Returns true if the subscription exists, otherwise false.</returns>
    Task<DatahubAzureSubscription> SubscriptionExistsAsync(string subscriptionId);

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

    /// <summary>
    /// Retrieves the number of remaining workspaces in a Datahub Azure subscription.
    /// </summary>
    /// <param name="subscriptionId">The GUID of the Datahub Azure subscription.</param>
    /// <returns>The task result contains the number of remaining workspaces.</returns>
    Task<int> NumberOfRemainingWorkspacesAsync(string subscriptionId);

    /// <summary>
    /// Retrieves the next available Datahub Azure subscription.
    /// </summary>
    /// <returns>
    /// The task result contains the next available Datahub Azure subscription.
    /// </returns>
    Task<DatahubAzureSubscription> NextSubscriptionAsync();

    /// <summary>
    /// Updates a Datahub Azure subscription in the database.
    /// </summary>
    /// <param name="subscription">The Datahub Azure subscription to update.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task UpdateSubscriptionAsync(DatahubAzureSubscription subscription);
}