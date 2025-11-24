using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Subscriptions;

/// <summary>
/// Represents an Azure subscription associated with Datahub workspaces.
/// Provides necessary information to manage and provision Azure resources.
/// </summary>
public class DatahubAzureSubscription
{
    /// <summary>
    /// Gets or sets the unique identifier for this Azure subscription record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier for the associated Azure subscription.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the subscription identifier for the Azure Subscription.
    /// </summary>
    public required string SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure Subscription.
    /// </summary>
    public required string SubscriptionName { get; set; }

    /// <summary>
    /// Gets or sets the user-friendly alias or nickname for the subscription.
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// Gets or sets the list of Datahub workspaces associated with this Azure subscription.
    /// </summary>
    public List<Datahub_Project> Workspaces { get; set; } = new();
}