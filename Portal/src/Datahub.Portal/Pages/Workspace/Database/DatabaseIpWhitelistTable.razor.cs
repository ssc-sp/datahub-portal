using System.Net;
using System.Net.Sockets;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Datahub.Core.Model.Context;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Database;

/// <summary>
/// Represents a table of IP addresses that are whitelisted in a database firewall rule.
///
/// TODO: This component is currently only used for PostgreSQL. It should be refactored to be more generic and reusable.
/// </summary>
public partial class DatabaseIpWhitelistTable
{
    /// <summary>
    /// Builds a PostgreSqlFlexibleServerResource object for the specified workspace acronym.
    /// </summary>
    /// <returns>A PostgreSqlFlexibleServerResource object.</returns>
    private async Task<PostgreSqlFlexibleServerResource> BuildPostgresSqlFlexibleServerResource()
    {
        var credential = new ClientSecretCredential(
            _portalConfiguration.AzureAd.TenantId,
            _portalConfiguration.AzureAd.InfraClientId,
            _portalConfiguration.AzureAd.InfraClientSecret);
        var client = new ArmClient(credential);

        var resourceGroupName =
            $"{_portalConfiguration.ResourcePrefix}_proj_{WorkspaceAcronym.ToLowerInvariant()}_{_portalConfiguration.Hosting.EnvironmentName}_rg";
        
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var subscriptionId = await RetrieveWorkspaceSubscriptionId(WorkspaceAcronym, context);
        var resourceProviderNamespace = "Microsoft.DBforPostgreSQL";
        var resourceType = "flexibleServers";
        var resourceName =
            $"{_portalConfiguration.ResourcePrefix}-{WorkspaceAcronym.ToLowerInvariant()}-psql-{_portalConfiguration.Hosting.EnvironmentName}";

        var resourceIdentifier =
            $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/{resourceProviderNamespace}/{resourceType}/{resourceName}";

        var postgresResource = client.GetPostgreSqlFlexibleServerResource(new ResourceIdentifier(resourceIdentifier));

        return postgresResource;
    }

    /// <summary>
    /// Retrieves the subscription ID for the specified workspace acronym.
    /// </summary>
    /// <param name="workspaceAcronym">The acronym of the workspace.</param>
    /// <param name="context">The database context to use for retrieving the workspace information.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the subscription ID.</returns>
    internal static async Task<string> RetrieveWorkspaceSubscriptionId(string workspaceAcronym,
        DatahubProjectDBContext context)
    {
        var workspace = await context.Projects
            .AsNoTracking()
            .Where(w => w.Project_Acronym_CD == workspaceAcronym)
            .Include(w => w.DatahubAzureSubscription)
            .FirstAsync();

        return workspace.DatahubAzureSubscription.SubscriptionId;
    }

    /// <summary>
    /// Handles the event when the commit edit button is clicked.
    /// </summary>
    /// <param name="args">The MouseEventArgs containing information about the event.</param>
    private void HandleCommitEditClicked(MouseEventArgs args)
    {
        _logger.LogInformation("Commit edit button clicked.");
        _snackbar.Add(Localizer["Sending IP address updated"], Severity.Info);
    }

    /// <summary>
    /// Handles the commit of a row edit in the DatabaseIpWhitelistTable.
    /// When a row name is edited, the old firewall rule is deleted and a new one is created.
    /// </summary>
    /// <param name="element">The edited element.</param>
    private void HandleRowEditCommit(object element)
    {
        var item = element as WhitelistIPAddressData;

        CreateOrUpdateIpAddress(item);

        // if it's only the name that has changed
        if (Equals(item?.StartIPAddress, _elementBeforeEdit?.StartIPAddress)
            && Equals(item?.EndIPAddress, _elementBeforeEdit?.EndIPAddress))
        {
            // clean up the old firewall rule
            DeleteIpAddress(_elementBeforeEdit);
        }

        _snackbar.Add(Localizer["IP address has been updated."], Severity.Success);

        _logger.LogInformation($"Item has been committed: {item?.Name}");
    }

    /// <summary>
    /// Filters the given WhitelistIPAddressData rule based on the filter string.
    /// </summary>
    /// <param name="rule">The WhitelistIPAddressData rule to filter.</param>
    /// <returns>Returns true if the rule matches the filter, false otherwise.</returns>
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
        var startIpAddress = _userIpAddress;
        var endIpAddress = _userIpAddress;
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

        var userWhitelistIpAddress = new WhitelistIPAddressData
        {
            Name = currentUser.Email,
            StartIPAddress = startIpAddress,
            EndIPAddress = endIpAddress
        };

        CreateOrUpdateIpAddress(userWhitelistIpAddress);

        _snackbar.Add(Localizer["Current IP address has been added. Changes may take 15 minutes to apply."],
            Severity.Success);
        _firewallRules.Add(userWhitelistIpAddress);
    }

    /// <summary>
    /// Prompt the user to enter a new IP address and validate it before adding it to the whitelist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task AddNewIpAddress()
    {
        // run a window.prompt to get the new IP address
        var newIpAddress = await _jsRuntime.InvokeAsync<string>("prompt",
            Localizer["Enter the new IP address (ex: 192.168.2.1) to whitelist:"].ToString());

        // check if the newIpAddress is a valid IP address
        if (IPAddress.TryParse(newIpAddress, out var ipAddress) &&
            ipAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            var newWhitelistIpAddress = new WhitelistIPAddressData
            {
                Name = @Localizer["Client IP Address {0}", Guid.NewGuid().ToString()[..8]],
                StartIPAddress = ipAddress,
                EndIPAddress = ipAddress
            };

            await CreateOrUpdateIpAddress(newWhitelistIpAddress);
            _snackbar.Add(Localizer["IP address {0} has been added. Changes may take 15 minutes to apply.", ipAddress],
                Severity.Success);
            _firewallRules.Add(newWhitelistIpAddress);
        }
        else
        {
            _snackbar.Add(Localizer["Invalid IP address."], Severity.Error);
        }
    }

    /// <summary>
    /// Deletes an IP address from the whitelist of a database firewall rule.
    /// </summary>
    /// <param name="whitelistIpAddressData">The IP address data to be deleted.</param>
    /// <param name="showSnackbar">Optional. If set to true, a snackbar message will be shown after the deletion. Default is false.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DeleteIpAddress(WhitelistIPAddressData whitelistIpAddressData, bool showSnackbar = false)
    {
        var postgresResource = await BuildPostgresSqlFlexibleServerResource();
        var rules = postgresResource.GetPostgreSqlFlexibleServerFirewallRules();
        var rule = rules.Get(whitelistIpAddressData?.Name);

        _logger.LogInformation($"Deleting firewall rule: {whitelistIpAddressData?.Name}");
        rule.Value.Delete(WaitUntil.Started);

        if (showSnackbar)
        {
            _snackbar.Add(Localizer["IP address {0} has been deleted.", whitelistIpAddressData?.Name ?? string.Empty],
                Severity.Success);
        }
    }

    /// <summary>
    /// Creates or updates a firewall rule with the specified whitelist IP address data.
    /// </summary>
    /// <param name="rule">The whitelist IP address data to create or update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CreateOrUpdateIpAddress(WhitelistIPAddressData rule)
    {
        var postgresResource = await BuildPostgresSqlFlexibleServerResource();
        var rules = postgresResource.GetPostgreSqlFlexibleServerFirewallRules();

        // until we support IP ranges, we will only use the start IP address
        rule.EndIPAddress = rule.StartIPAddress;

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

    /// <summary>
    /// Handles the cancellation of an edit operation on a row in the DatabaseIpWhitelistTable.
    /// </summary>
    /// <param name="whitelistRule">The object representing the whitelist rule being edited.</param>
    private void HandleRowEditCancel(object whitelistRule)
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