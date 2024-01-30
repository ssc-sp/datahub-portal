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
        
        client.
        return postgresResource;
    }
    
    private async Task ItemHasBeenCommitted(object element)
    {
        var item = element as WhitelistIPAddressData;
        
        await CreateOrUpdateIpAddress(item);
        _snackbar.Add(Localizer["Item has been committed"], Severity.Success);
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

        await CreateOrUpdateIpAddress(userWhitelistIpAddress);
    }
    
    private async Task CreateOrUpdateIpAddress(WhitelistIPAddressData rule)
    {
        var postgresResource = GetPostgreSqlFlexibleServerResource();

        var rules = postgresResource.GetPostgreSqlFlexibleServerFirewallRules();
        await rules.CreateOrUpdateAsync(WaitUntil.Started, rule.Name, rule.FlexibleFirewallRuleData);
    }
}

