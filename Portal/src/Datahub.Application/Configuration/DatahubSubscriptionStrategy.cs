using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;

namespace Datahub.Application.Configuration
{
    public class DatahubSubscriptionStrategy(DatahubPortalConfiguration portalConfiguration)
    {
        public int NumberOfWorkspacesRemaining(List<Datahub_Project> workspacesUsingSubscription)
        {
            return portalConfiguration.Hosting.WorkspaceCountPerAzureSubscription - workspacesUsingSubscription.Count;
        }

        public DatahubAzureSubscription NextSubscription(List<DatahubAzureSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                var remainingWorkspaces = NumberOfWorkspacesRemaining(subscription.Workspaces);
                if (remainingWorkspaces > 0)
                {
                    return subscription;
                }
            }

            throw new InvalidOperationException("No subscriptions available with remaining workspaces.");
        }
    }
}