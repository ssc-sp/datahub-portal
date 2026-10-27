extern alias AzIdentity;
using System.Net;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Datahub.Application.Services.Security;
using Datahub.Core.Extensions;
using Datahub.Core.Model.Context;
using Datahub.Shared.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Database;

/// <summary>
/// Represents a table of IP addresses that are whitelisted in a database firewall rule.
///
/// TODO: This component is currently only used for PostgreSQL. It should be refactored to be more generic and reusable.
/// </summary>
public partial class DatabaseIpWhitelistTable
{
    [Inject] private IServiceProvider _serviceProvider { get; set; } = null!;

    private ISystemTokenCredentialService TokenCredentialService =>
        _serviceProvider.GetRequiredKeyedService<ISystemTokenCredentialService>(SystemTokenCredentialServiceKeys.Infra);

    /// <summary>
    /// Builds a PostgreSqlFlexibleServerResource object for the specified workspace acronym.
    /// </summary>
    /// <returns>A PostgreSqlFlexibleServerResource object.</returns>
    private async Task<PostgreSqlFlexibleServerResource> BuildPostgresSqlFlexibleServerResource()
    {
        var client = new ArmClient(TokenCredentialService.GetTokenCredential());

        var resourceGroupName =
            $"{_portalConfiguration.ResourcePrefix}_proj_{WorkspaceAcronym.ToLowerInvariant()}_{_portalConfiguration.Hosting.EnvironmentName}_rg";
        
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var subscriptionId = await RetrieveWorkspaceSubscriptionId(WorkspaceAcronym, context);
        var dbResource = await context.Project_Resources2.AsNoTracking().Include(p => p.Project).FirstAsync(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres) && r.Project.Project_Acronym_CD == WorkspaceAcronym);
        var pgsqlId = dbResource.GetPostgresId();

        var postgresResource = client.GetPostgreSqlFlexibleServerResource(new ResourceIdentifier(pgsqlId));

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
    /// Adds the current IP address to the whitelist.
    /// </summary>
    /// <returns>Void</returns>
    private async Task AddCurrentIpAddress()
    {
        if (_userIpAddress is null)
        {
            return;
        }

        var startIpAddress = _userIpAddress;
        var endIpAddress = _userIpAddress;
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();

        var userWhitelistIpAddress = new WhitelistIPAddressData
        {
            Name = currentUser.Email,
            StartIPAddress = startIpAddress,
            EndIPAddress = endIpAddress
        };

        await CreateOrUpdateIpAddress(userWhitelistIpAddress);

        _snackbar.Add(Localizer["Current IP address has been added. Changes may take 15 minutes to apply."],
            Severity.Success);
        _firewallRules.Add(userWhitelistIpAddress);
    }

    private void StartAddingIpRange()
    {
        _ruleFormMode = RuleFormMode.Add;
        _ruleName = string.Empty;
        _startIpAddress = string.Empty;
        _endIpAddress = string.Empty;
        _editingOriginalName = string.Empty;
        _ruleFormError = string.Empty;
    }

    private void StartEditingSelectedIpRange()
    {
        var selectedRule = SelectedRule;
        if (selectedRule is null)
        {
            return;
        }

        _ruleFormMode = RuleFormMode.Edit;
        _ruleName = selectedRule.Name;
        _startIpAddress = selectedRule.StartIPAddress.ToString();
        _endIpAddress = selectedRule.EndIPAddress.ToString();
        _editingOriginalName = selectedRule.Name;
        _ruleFormError = string.Empty;
    }

    private async Task SaveIpRange()
    {
        if (!TryValidateIpRange(out var startIpAddress, out var endIpAddress))
        {
            return;
        }

        _savingRule = true;
        try
        {
            var isAdding = _ruleFormMode is RuleFormMode.Add;
            var updatedRule = new WhitelistIPAddressData
            {
                Name = isAdding
                    ? Localizer["Client IP Address {0}", Guid.NewGuid().ToString()[..8]]
                    : _ruleName,
                StartIPAddress = startIpAddress,
                EndIPAddress = endIpAddress
            };

            await CreateOrUpdateIpAddress(updatedRule);

            if (isAdding)
            {
                _firewallRules.Add(updatedRule);
                _snackbar.Add(
                    Localizer[
                        "IP address(es) {0} - {1} have been added. Changes may take 15 minutes to apply.",
                        startIpAddress,
                        endIpAddress],
                    Severity.Success);
            }
            else
            {
                var originalRule = _firewallRules.First(rule => rule.Name == _editingOriginalName);
                if (!string.Equals(originalRule.Name, updatedRule.Name, StringComparison.Ordinal))
                {
                    await DeleteIpAddress(originalRule);
                }
                else
                {
                    _firewallRules.Remove(originalRule);
                }

                _firewallRules.Add(updatedRule);
                _selectedRuleName = updatedRule.Name;
                _snackbar.Add(Localizer["IP address has been updated."], Severity.Success);
                _snackbar.Add(Localizer["Sending IP address updated"], Severity.Info);
            }

            CancelIpRangeForm();
        }
        finally
        {
            _savingRule = false;
        }
    }

    private bool TryValidateIpRange(out IPAddress startIpAddress, out IPAddress endIpAddress)
    {
        startIpAddress = IPAddress.None;
        endIpAddress = IPAddress.None;

        if (_ruleFormMode is RuleFormMode.Edit && string.IsNullOrWhiteSpace(_ruleName))
        {
            _ruleFormError = Localizer["Name is required"];
            return false;
        }

        if (!IPAddress.TryParse(_startIpAddress, out startIpAddress))
        {
            _ruleFormError = Localizer["Invalid IP address."];
            return false;
        }

        if (string.IsNullOrWhiteSpace(_endIpAddress))
        {
            endIpAddress = startIpAddress;
        }
        else if (!IPAddress.TryParse(_endIpAddress, out endIpAddress))
        {
            _ruleFormError = Localizer["Invalid IP address."];
            return false;
        }

        if (!IsValidRange(startIpAddress, endIpAddress))
        {
            _ruleFormError = Localizer["Invalid IP range."];
            return false;
        }

        _ruleFormError = string.Empty;
        return true;
    }

    private static bool IsValidRange(IPAddress startIpAddress, IPAddress endIpAddress)
    {
        var startBytes = startIpAddress.GetAddressBytes();
        var endBytes = endIpAddress.GetAddressBytes();

        if (startBytes.Length < 4 || endBytes.Length < 4)
        {
            return false;
        }

        for (var index = 0; index < 3; index++)
        {
            if (startBytes[index] != endBytes[index])
            {
                return false;
            }
        }

        return startBytes[3] <= endBytes[3];
    }

    private void CancelIpRangeForm()
    {
        _ruleFormMode = RuleFormMode.None;
        _ruleName = string.Empty;
        _startIpAddress = string.Empty;
        _endIpAddress = string.Empty;
        _editingOriginalName = string.Empty;
        _ruleFormError = string.Empty;
    }

    /// <summary>
    /// Deletes the currently selected firewall rule.
    /// </summary>
    private async Task DeleteSelectedIpAddress()
    {
        var selectedRule = SelectedRule;
        if (selectedRule is null)
        {
            return;
        }

        await DeleteIpAddress(selectedRule, true);
        _selectedRuleName = null;
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

        _firewallRules.RemoveAll(rule => rule.Name == whitelistIpAddressData.Name);
        StateHasChanged();

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
        // rule.EndIPAddress = rule.StartIPAddress;

        _logger.LogInformation($"Creating or updating firewall rule: {rule.Name}");
        rules.CreateOrUpdate(WaitUntil.Started, rule.Name, rule.FlexibleFirewallRuleData);
        _logger.LogInformation($"Firewall rule has been created or updated: {rule.Name}");
    }

}
