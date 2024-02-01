using System.Net;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Database;

public partial class DatabaseIpWhitelistTable
{
    private PostgreSqlFlexibleServerResource GetPostgreSqlFlexibleServerResource()
    {
        var credential = new ClientSecretCredential(
            _portalConfiguration.AzureAd.TenantId, 
            _portalConfiguration.AzureAd.InfraClientId, 
            _portalConfiguration.AzureAd.InfraClientSecret);
        var client = new ArmClient(credential);

        var resourceGroupName = $"fsdh_proj_{WorkspaceAcronym.ToLowerInvariant()}_dev_rg";
        var subscriptionId = _portalConfiguration.AzureAd.SubscriptionId;
        var resourceProviderNamespace = "Microsoft.DBforPostgreSQL";
        var resourceType = "flexibleServers";
        var resourceName = $"fsdh-{WorkspaceAcronym.ToLowerInvariant()}-psql-dev";

        var resourceIdentifier = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/{resourceProviderNamespace}/{resourceType}/{resourceName}";

        var postgresResource = client.GetPostgreSqlFlexibleServerResource(new ResourceIdentifier(resourceIdentifier));
        
        return postgresResource;
    }
    
    private void ItemHasBeenCommitted(object element)
    {
        var item = element as WhitelistIPAddressData;
        
        CreateOrUpdateIpAddress(item);
        _snackbar.Add(Localizer["IP address has been updated."], Severity.Success);
        
        _logger.LogInformation($"Item has been committed: {item?.Name}");
    }
      
    private bool FilterFunc(WhitelistIPAddressData rule)
    {
        if (string.IsNullOrWhiteSpace(_filterString))
            return true;
        if (rule.Name.Contains(_filterString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (rule.StartIPAddress.ToString().Contains(_filterString, StringComparison.OrdinalIgnoreCase))
            return true;
        if (rule.EndIPAddress.ToString().Contains(_filterString, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Adds the current IP address to the whitelist.
    /// </summary>
    /// <returns>Void</returns>
    private async Task AddCurrentIpAddress()
    {
        var startIpAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
        var endIpAddress = new IPAddress(new byte[] { 192, 168, 1, 1 });
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
        
        var userWhitelistIpAddress = new WhitelistIPAddressData
        {
            Name = currentUser.Email,
            StartIPAddress = startIpAddress,
            EndIPAddress = endIpAddress
        };

        CreateOrUpdateIpAddress(userWhitelistIpAddress);
    }

    /// <summary>
    /// Creates or updates a firewall rule for whitelisting an IP address.
    /// </summary>
    /// <param name="rule">The whitelist IP address data.</param>
    private void CreateOrUpdateIpAddress(WhitelistIPAddressData rule)
    {
        var postgresResource = GetPostgreSqlFlexibleServerResource();
        var rules = postgresResource.GetPostgreSqlFlexibleServerFirewallRules();
        
        _logger.LogInformation($"Creating or updating firewall rule: {rule.Name}");
        rules.CreateOrUpdate(WaitUntil.Started, rule.Name, rule.FlexibleFirewallRuleData);
        _logger.LogInformation($"Firewall rule has been created or updated: {rule.Name}");
    }

    /// <summary>
    /// Backs up an item by creating a copy of it in memory.
    /// </summary>
    /// <param name="whitelistRule">The item to be backed up.</param>
    private void BackupItem(object whitelistRule)
    {
        _elementBeforeEdit = new WhitelistIPAddressData
        {
            Name = ((WhitelistIPAddressData)whitelistRule).Name,
            StartIPAddress = ((WhitelistIPAddressData)whitelistRule).StartIPAddress,
            EndIPAddress = ((WhitelistIPAddressData)whitelistRule).EndIPAddress
        };
        
        _logger.LogInformation($"Item has been backed up: {_elementBeforeEdit?.Name}");
    }

    private void ResetItemToOriginalValues(object whitelistRule)
    {
        if (whitelistRule is not WhitelistIPAddressData item)
        {
            return;
        }

        item.Name = _elementBeforeEdit.Name;
        item.StartIPAddress = _elementBeforeEdit.StartIPAddress;
        item.EndIPAddress = _elementBeforeEdit.EndIPAddress;

        _logger.LogInformation($"Item has been reset to original values: {item.Name}");
    }
}

