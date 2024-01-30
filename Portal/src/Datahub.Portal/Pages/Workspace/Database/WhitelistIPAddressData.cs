using System.Net;
using System.Text.RegularExpressions;
using Azure.ResourceManager.PostgreSql.FlexibleServers;

namespace Datahub.Portal.Pages.Workspace.Database;

// ReSharper disable once InconsistentNaming
public partial class WhitelistIPAddressData
{
    public WhitelistIPAddressData()
    {
    }

    public WhitelistIPAddressData(PostgreSqlFlexibleServerFirewallRuleData firewallRuleData)
    {
        Name = firewallRuleData.Name;
        StartIPAddress = firewallRuleData.StartIPAddress;
        EndIPAddress = firewallRuleData.EndIPAddress;
    }

    private string _name;
    public string Name
    {
        get { return _name; }
        set
        {
            _name =  RemoveInvalidCharacters(value);
        }
    }

    public IPAddress StartIPAddress { get; set; }
    public IPAddress EndIPAddress { get; set; }

    public PostgreSqlFlexibleServerFirewallRuleData FlexibleFirewallRuleData => new(StartIPAddress, EndIPAddress);

    private string RemoveInvalidCharacters(string input)
    {
        // Define the regex pattern based on the specified rules
        const string pattern = @"^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,78}[a-zA-Z0-9_]$";
        // Remove characters that don't match the pattern
        var cleanedInput = FirewallRegex().Replace(input, "");

        return cleanedInput;
    }

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_.-]{0,78}[a-zA-Z0-9_]$")]
    private static partial Regex FirewallRegex();
}