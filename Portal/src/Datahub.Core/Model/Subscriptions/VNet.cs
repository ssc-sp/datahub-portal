namespace Datahub.Core.Model.Subscriptions;

/// <summary>
/// Represents an Azure Virtual Network (VNet) belonging to an Azure subscription.
/// A VNet can contain multiple subnets shared across workspaces.
/// </summary>
public class VNet
{
    /// <summary>
    /// Gets or sets the unique identifier for this VNet record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the Azure resource identifier of the VNet.
    /// </summary>
    public required string VNetId { get; set; }

    /// <summary>
    /// Gets or sets the name of the VNet (e.g. "workspace-vnet").
    /// </summary>
    public required string VNetName { get; set; }

    /// <summary>
    /// Gets or sets the foreign key referencing the Azure subscription that owns this VNet.
    /// </summary>
    public int SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the Azure subscription that owns this VNet.
    /// </summary>
    public DatahubAzureSubscription Subscription { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of subnets contained within this VNet.
    /// </summary>
    public List<Subnet> Subnets { get; set; } = new();
}
