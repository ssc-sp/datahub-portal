using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Subscriptions;

public class DatahubAzureSubscription
{
        public int Id { get; set; }
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the subscription identifier for the Azure Subscription.
        /// </summary>
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string Nickname { get; set; }

        public List<Datahub_Project> Workspaces { get; set; }
}