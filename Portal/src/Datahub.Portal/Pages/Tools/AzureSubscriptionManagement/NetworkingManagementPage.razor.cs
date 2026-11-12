using Datahub.Application.Services;
using Datahub.Application.Services.Subscriptions;
using Datahub.Core.Model.Subscriptions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Datahub.Portal.Pages.Tools.AzureSubscriptionManagement;

public partial class NetworkingManagementPage : ComponentBase
{
    // ── State ──────────────────────────────────────────────────────────────
    private List<VNet> _vnets = new();
    private List<Subnet> _subnets = new();
    private IReadOnlyDictionary<int, SubnetGroupUsage> _subnetUsage = new Dictionary<int, SubnetGroupUsage>();
    private List<DatahubAzureSubscription> _subscriptions = new();

    private VNet? _selectedVNet;
    private VNet _vnetBeforeEdit = new() { VNetId = string.Empty, VNetName = string.Empty };

    // Attach / release
    private bool _showAttach;
    private bool _showRelease;
    private Subnet? _groupForAction;
    private string _attachAcronym = string.Empty;
    private string? _actionError;

    // Update range
    private bool _showUpdateRange;
    private int _rangeFirst;
    private int _rangeLast;
    private string? _rangeError;

    // Add VNet dialog
    private bool _showAddVNet;
    private bool _vnetFormValid;
    private MudForm _vnetForm = null!;
    private VNet _newVNet = new() { VNetId = string.Empty, VNetName = string.Empty };

    // ── Lifecycle ──────────────────────────────────────────────────────────
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        _subscriptions = await _subscriptionService.ListSubscriptionsAsync();
        await RefreshVNets();
    }

    private async Task RefreshVNets()
    {
        _vnets = await _networkingService.ListVNetsAsync();
    }

    private async Task SelectVNet(VNet vnet)
    {
        _selectedVNet = vnet;
        await LoadSubnetsAsync(vnet.Id);
    }

    private async Task LoadSubnetsAsync(int vnetId)
    {
        _subnets = await _networkingService.ListSubnetsAsync(vnetId);
        _subnetUsage = await _subnetPoolService.GetSubnetGroupUsageAsync(vnetId);
    }

    // ── Subnet group usage ─────────────────────────────────────────────────
    private SubnetGroupUsage GetUsage(Subnet subnet) =>
        _subnetUsage.TryGetValue(subnet.Id, out var usage) ? usage : new SubnetGroupUsage(SubnetGroupStatus.Available);

    private string GetStatusLabel(SubnetGroupUsage usage) => usage.Status switch
    {
        SubnetGroupStatus.InUse => Localizer["In use"],
        SubnetGroupStatus.CoolingDown => Localizer["Cooling down until {0} UTC", $"{usage.AvailableAtUtc:yyyy-MM-dd HH:mm}"],
        _ => Localizer["Available"]
    };

    private static Color GetStatusColor(SubnetGroupStatus status) => status switch
    {
        SubnetGroupStatus.InUse => Color.Warning,
        SubnetGroupStatus.CoolingDown => Color.Info,
        _ => Color.Success
    };

    // ── VNet helpers ───────────────────────────────────────────────────────
    private string GetSubscriptionLabel(int subscriptionId)
    {
        var sub = _subscriptions.FirstOrDefault(s => s.Id == subscriptionId);
        return sub is null ? subscriptionId.ToString() : $"{sub.Nickname} ({sub.SubscriptionName})";
    }

    private void OpenAddVNetDialog()
    {
        _newVNet = new VNet { VNetId = string.Empty, VNetName = string.Empty };
        _showAddVNet = true;
    }

    private async Task SubmitAddVNet()
    {
        await _vnetForm.Validate();
        if (!_vnetFormValid) return;

        try
        {
            await _networkingService.AddVNetAsync(_newVNet);
            _snackbar.Add(Localizer["VNet added successfully"], Severity.Success);
            _showAddVNet = false;
            await RefreshVNets();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add VNet {VNetName}", _newVNet.VNetName);
            _snackbar.Add(Localizer["Failed to add VNet"], Severity.Error);
        }
    }

    private void BackupVNet(object obj)
    {
        if (obj is not VNet v) return;
        _vnetBeforeEdit = new VNet { Id = v.Id, VNetId = v.VNetId, VNetName = v.VNetName, SubscriptionId = v.SubscriptionId };
    }

    private void CancelVNetEdit(object obj)
    {
        if (obj is not VNet v) return;
        v.VNetId = _vnetBeforeEdit.VNetId;
        v.VNetName = _vnetBeforeEdit.VNetName;
    }

    private void CommitVNetEdit(object obj)
    {
        if (obj is not VNet v) return;
        Task.Run(async () =>
        {
            try
            {
                await _networkingService.UpdateVNetAsync(v);
                _snackbar.Add(Localizer["VNet updated"], Severity.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update VNet {Id}", v.Id);
                _snackbar.Add(Localizer["Failed to update VNet"], Severity.Error);
            }
        });
    }

    private async Task DeleteVNet(VNet vnet)
    {
        try
        {
            await _networkingService.DeleteVNetAsync(vnet.Id);
            _snackbar.Add(Localizer["VNet deleted"], Severity.Success);
            if (_selectedVNet?.Id == vnet.Id) _selectedVNet = null;
            await RefreshVNets();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete VNet {Id}", vnet.Id);
            _snackbar.Add(Localizer["Failed to delete VNet"], Severity.Error);
        }
    }

    // ── Subnet group range ─────────────────────────────────────────────────
    private int RangeStart => _subnets.Count == 0 ? 0 : _subnets.Min(s => s.SubnetGroup);
    private int RangeEnd => _subnets.Count == 0 ? 0 : _subnets.Max(s => s.SubnetGroup);
    private int CountWithStatus(SubnetGroupStatus status) => _subnets.Count(s => GetUsage(s).Status == status);

    private void OpenUpdateRangeDialog()
    {
        _rangeFirst = _subnets.Count == 0 ? 1 : RangeStart;
        _rangeLast = _subnets.Count == 0 ? 1 : RangeEnd;
        _rangeError = null;
        _showUpdateRange = true;
    }

    private async Task SubmitUpdateRange()
    {
        try
        {
            var (added, removed) = await _subnetPoolService.UpdateSubnetGroupRangeAsync(_selectedVNet!.Id, _rangeFirst, _rangeLast);
            _logger.LogInformation("Subnet group range of VNet {VNetId} set to {First}-{Last} ({Added} added, {Removed} removed)",
                _selectedVNet!.Id, _rangeFirst, _rangeLast, added, removed);
            _snackbar.Add(Localizer["Range set to groups {0}–{1}: {2} added, {3} removed", _rangeFirst, _rangeLast, added, removed], Severity.Success);
            _showUpdateRange = false;
            await LoadSubnetsAsync(_selectedVNet!.Id);
            await RefreshVNets();
        }
        catch (InvalidOperationException ex)
        {
            // The service's messages are fixed English text, so they can be looked up in the localizer.
            _rangeError = Localizer[ex.Message];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update the subnet group range to {First}-{Last}", _rangeFirst, _rangeLast);
            _rangeError = Localizer["Failed to update the range."];
        }
    }

    // ── Attach / release subnet groups ─────────────────────────────────────
    private void OpenAttachDialog(Subnet subnet)
    {
        _groupForAction = subnet;
        _attachAcronym = string.Empty;
        _actionError = null;
        _showAttach = true;
    }

    private void OpenReleaseDialog(Subnet subnet)
    {
        _groupForAction = subnet;
        _actionError = null;
        _showRelease = true;
    }

    private async Task SubmitAttach()
    {
        if (_groupForAction is null) return;
        try
        {
            await _subnetPoolService.AttachSubnetGroupAsync(_groupForAction.Id, _attachAcronym);
            _snackbar.Add(Localizer["Subnet group {0} attached to {1}", _groupForAction.SubnetGroup, _attachAcronym.Trim().ToUpperInvariant()], Severity.Success);
            _showAttach = false;
            await LoadSubnetsAsync(_selectedVNet!.Id);
        }
        catch (InvalidOperationException ex)
        {
            _actionError = Localizer[ex.Message];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach subnet group {Id} to {Acronym}", _groupForAction.Id, _attachAcronym);
            _actionError = Localizer["Failed to attach the subnet group."];
        }
    }

    private async Task SubmitRelease()
    {
        if (_groupForAction is null) return;
        try
        {
            await _subnetPoolService.ReleaseSubnetGroupAsync(_groupForAction.Id);
            _logger.LogInformation("Subnet group {Group} released manually", _groupForAction.SubnetGroup);
            _snackbar.Add(Localizer["Subnet group {0} released", _groupForAction.SubnetGroup], Severity.Success);
            _showRelease = false;
            await LoadSubnetsAsync(_selectedVNet!.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release subnet group {Id}", _groupForAction.Id);
            _actionError = Localizer["Failed to release the subnet group."];
        }
    }
}
