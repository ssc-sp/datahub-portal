using Datahub.Application.Services;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Settings;

public partial class WorkspaceSettingsPage
{
    /// <summary>
    /// Handles the change in the PreventAutoDelete setting.
    /// </summary>
    /// <param name="arg">The new value for the PreventAutoDelete setting.</param>
    internal void HandlePreventAutoDeleteChange(bool arg)
    {
        _preventAutoDelete = arg;
    }

    /// <summary>
    /// Handles the change in the budget value.
    /// </summary>
    /// <param name="arg">The new budget value to be applied.</param>
    internal void HandleBudgetChanged(decimal arg)
    {
        _budget = arg;
    }

    /// <summary>
    /// Saves the changes made to the workspace model, updates the database,
    /// and triggers a Terraform update if necessary.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    internal async Task SaveChanges()
    {
        if(!HasChanged)
        {
            _snackbar.Add(Localizer["No changes to save"], Severity.Info);
            return;
        }
        _updateInProgress = true;
        await InvokeAsync(StateHasChanged);
        
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        context.Attach(_workspace);
        context.Entry(_workspace).State = EntityState.Modified;
        var queueUpTerraformUpdate = RequiresTerraformUpdate;
        
        SetWorkspacePropertiesFromForm();
        await context.TrackSaveChangesAsync(_auditingService);

        if (queueUpTerraformUpdate)
        {
            await RunTerraformUpdate();
        }
        _snackbar.Add(Localizer["Changes to workspace have been saved"], Severity.Info);

        _updateInProgress = false;
        SetFormPropertiesFromWorkspace();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Executes the Terraform update process asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal async Task RunTerraformUpdate()
    {
        var currentUser = await _userInformationService.GetCurrentPortalUserAsync();
        var template = new TerraformTemplate(TerraformTemplate.VariableUpdate, TerraformStatus.CreateRequested);
        
        await _requestManagementService.HandleTerraformRequestServiceAsync(_workspace, template, currentUser);
        _snackbar.Add(Localizer["Queued Terraform update for workspace changes"], Severity.Info);
    }

    /// <summary>
    /// Updates the workspace model properties based on the current form data.
    /// Sets the PreventAutoDelete and Project_Budget properties of the workspace.
    /// </summary>
    private void SetWorkspacePropertiesFromForm()
    {
        _workspace.PreventAutoDelete = _preventAutoDelete;
        _workspace.Project_Budget = _budget;
    }

    /// <summary>
    /// Updates the form properties based on the current properties of the workspace model.
    /// </summary>
    private void SetFormPropertiesFromWorkspace()
    {
        _preventAutoDelete = _workspace.PreventAutoDelete;
        _budget = _workspace.Project_Budget ?? 0;
    }
}