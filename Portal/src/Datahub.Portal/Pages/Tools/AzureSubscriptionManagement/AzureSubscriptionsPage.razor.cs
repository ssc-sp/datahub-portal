using Datahub.Core.Model.Subscriptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Datahub.Portal.Pages.Tools.AzureSubscriptionManagement;

public partial class AzureSubscriptionsPage : ComponentBase
{

    private async Task HandleSubscriptionSubmitted(DatahubAzureSubscription subscription)
    {
        _logger.LogInformation("Adding Azure subscription {SubscriptionId}.", subscription.SubscriptionId);
        _snackbar.Add(Localizer["Adding Azure subscription"], Severity.Info);
        try
        {
            await _datahubAzureSubscriptionService.AddSubscriptionAsync(subscription);
        
            _logger.LogInformation("Azure subscription {SubscriptionId} added.", subscription.SubscriptionId);
            _snackbar.Add(Localizer["Azure subscription added"], Severity.Success);
            StateHasChanged();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add Azure subscription {SubscriptionId}.", subscription.SubscriptionId);
            _snackbar.Add(Localizer["Failed to add Azure subscription"], Severity.Error);
        }
    }
    
    private void HandleCommitEditClicked(MouseEventArgs args)
    {
        _logger.LogInformation("Commit edit button clicked.");
        _snackbar.Add(Localizer["Sending Azure subscription updated"], Severity.Info);
    }
    
    private void HandleRowEditCommit(object element)
    {
        if (element is not DatahubAzureSubscription datahubAzureSubscription)
        {
            return;
        }
        
        _logger.LogInformation("Committing row edit for Azure subscription {SubscriptionId}.", datahubAzureSubscription.SubscriptionId);
        _snackbar.Add(Localizer["Committing Azure subscription update"], Severity.Info);
        
        Task.Run(async () =>
        {
            await _datahubAzureSubscriptionService.UpdateSubscriptionAsync(datahubAzureSubscription);
            _logger.LogInformation("Row edit committed for Azure subscription {SubscriptionId}.", datahubAzureSubscription.SubscriptionId);
            _snackbar.Add(Localizer["Azure subscription updated"], Severity.Info);
        });
    }

    private void BackupItem(object subscription)
    {
        _elementBeforeEdit = new DatahubAzureSubscription
        {
            Id = ((DatahubAzureSubscription)subscription).Id,
            TenantId = ((DatahubAzureSubscription)subscription).TenantId,
            SubscriptionId = ((DatahubAzureSubscription)subscription).SubscriptionId,
            Nickname = ((DatahubAzureSubscription)subscription).Nickname,
        };
        
        _logger.LogInformation("Backing up item {SubscriptionId} for editing.", ((DatahubAzureSubscription)subscription).SubscriptionId);
    }
    
    private void HandleRowEditCancel(object element)
    {
        if(element is not DatahubAzureSubscription item)
        {
            return;
        }
        
        item.Id = _elementBeforeEdit.Id;
        item.TenantId = _elementBeforeEdit.TenantId;
        item.SubscriptionId = _elementBeforeEdit.SubscriptionId;
        item.Nickname = _elementBeforeEdit.Nickname;
        
        _logger.LogInformation("Row edit cancelled for Azure subscription {SubscriptionId}.", item.SubscriptionId);
    }
}