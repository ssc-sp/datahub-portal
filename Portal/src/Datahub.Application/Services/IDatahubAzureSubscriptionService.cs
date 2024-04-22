using Datahub.Core.Model.Subscriptions;

namespace Datahub.Application.Services;

public interface IDatahubAzureSubscriptionService
{
    Task<List<DatahubAzureSubscription>> ListSubscriptionsAsync();
}