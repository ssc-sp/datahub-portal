using System.Net;
using System.Text.RegularExpressions;
using Azure.ResourceManager.PostgreSql.FlexibleServers;

namespace Datahub.Portal.Pages.Workspace.Database;

// ReSharper disable once InconsistentNaming
/// <summary>
/// Represents an IP address data used for whitelisting in a database firewall rule.
/// </summary>
public partial class WhitelistIPAddressData
{
    private string _name;

    public WhitelistIPAddressData()
    {
    }

    public WhitelistIPAddressData(PostgreSqlFlexibleServerFirewallRuleData firewallRuleData)
    {
        Name = firewallRuleData.Name;
        StartIPAddress = firewallRuleData.StartIPAddress;
        EndIPAddress = firewallRuleData.EndIPAddress;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(value));

            const int maxLength = 63;
            
            var trimmedName = value.Length > maxLength ? value.Substring(0, maxLength) : value;

            // replace all non-alphanumeric characters with empty string
            trimmedName = AzureFirewallNamingRegex().Replace(trimmedName, "");
            trimmedName = trimmedName.Trim("-_ ".ToCharArray());
            _name = trimmedName;
        }
    }

    public IPAddress StartIPAddress { get; set; }
    public IPAddress EndIPAddress { get; set; }

    public PostgreSqlFlexibleServerFirewallRuleData FlexibleFirewallRuleData => new(StartIPAddress, EndIPAddress);

    [GeneratedRegex(@"[^a-zA-Z0-9\-_]")]
    private static partial Regex AzureFirewallNamingRegex();
}