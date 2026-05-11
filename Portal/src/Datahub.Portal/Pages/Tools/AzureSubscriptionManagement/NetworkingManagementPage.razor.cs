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
    private List<DatahubAzureSubscription> _subscriptions = new();

    private VNet? _selectedVNet;
    private VNet _vnetBeforeEdit = new() { VNetId = string.Empty, VNetName = string.Empty };
    private Subnet _subnetBeforeEdit = new() { SubnetName = string.Empty };

    // Add dialogs
    private bool _showAddVNet;
    private bool _showAddSubnet;
    private bool _vnetFormValid;
    private bool _subnetFormValid;
    private MudForm _vnetForm = null!;
    private MudForm _subnetForm = null!;

    private VNet _newVNet = new() { VNetId = string.Empty, VNetName = string.Empty };
    private Subnet _newSubnet = new() { SubnetName = string.Empty };

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
        _subnets = await _networkingService.ListSubnetsAsync(vnet.Id);
    }

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
            _snackbar.Add("VNet added successfully", Severity.Success);
            _showAddVNet = false;
            await RefreshVNets();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add VNet {VNetName}", _newVNet.VNetName);
            _snackbar.Add("Failed to add VNet", Severity.Error);
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
                _snackbar.Add("VNet updated", Severity.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update VNet {Id}", v.Id);
                _snackbar.Add("Failed to update VNet", Severity.Error);
            }
        });
    }

    private async Task DeleteVNet(VNet vnet)
    {
        try
        {
            await _networkingService.DeleteVNetAsync(vnet.Id);
            _snackbar.Add("VNet deleted", Severity.Success);
            if (_selectedVNet?.Id == vnet.Id) _selectedVNet = null;
            await RefreshVNets();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete VNet {Id}", vnet.Id);
            _snackbar.Add("Failed to delete VNet", Severity.Error);
        }
    }

    // ── Subnet helpers ─────────────────────────────────────────────────────
    private void OpenAddSubnetDialog()
    {
        _newSubnet = new Subnet { SubnetName = string.Empty, VNetId = _selectedVNet!.Id };
        _showAddSubnet = true;
    }

    private async Task SubmitAddSubnet()
    {
        await _subnetForm.Validate();
        if (!_subnetFormValid) return;

        try
        {
            await _networkingService.AddSubnetAsync(_newSubnet);
            _snackbar.Add("Subnet added successfully", Severity.Success);
            _showAddSubnet = false;
            _subnets = await _networkingService.ListSubnetsAsync(_selectedVNet!.Id);
            await RefreshVNets(); // refresh subnet count
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add subnet {SubnetName}", _newSubnet.SubnetName);
            _snackbar.Add("Failed to add subnet", Severity.Error);
        }
    }

    private void BackupSubnet(object obj)
    {
        if (obj is not Subnet s) return;
        _subnetBeforeEdit = new Subnet { Id = s.Id, SubnetName = s.SubnetName, AddressPrefix = s.AddressPrefix, SubnetGroup = s.SubnetGroup, VNetId = s.VNetId };
    }

    private void CancelSubnetEdit(object obj)
    {
        if (obj is not Subnet s) return;
        s.SubnetName = _subnetBeforeEdit.SubnetName;
        s.AddressPrefix = _subnetBeforeEdit.AddressPrefix;
        s.SubnetGroup = _subnetBeforeEdit.SubnetGroup;
    }

    private void CommitSubnetEdit(object obj)
    {
        if (obj is not Subnet s) return;
        Task.Run(async () =>
        {
            try
            {
                await _networkingService.UpdateSubnetAsync(s);
                _snackbar.Add("Subnet updated", Severity.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update subnet {Id}", s.Id);
                _snackbar.Add("Failed to update subnet", Severity.Error);
            }
        });
    }

    private async Task DeleteSubnet(Subnet subnet)
    {
        try
        {
            await _networkingService.DeleteSubnetAsync(subnet.Id);
            _snackbar.Add("Subnet deleted", Severity.Success);
            _subnets = await _networkingService.ListSubnetsAsync(_selectedVNet!.Id);
            await RefreshVNets();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete subnet {Id}", subnet.Id);
            _snackbar.Add("Failed to delete subnet", Severity.Error);
        }
    }
}
