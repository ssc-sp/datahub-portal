using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Settings;

public partial class WorkspaceSharedKeyAccessControl : ComponentBase
{
    /// <summary>
    /// Builds and configures an instance of the Azure ResourceManager ArmClient using the given portal configuration.
    /// </summary>
    /// <param name="portalConfiguration">The configuration settings required to authenticate and initialize the ArmClient.</param>
    /// <returns>An initialized instance of the ArmClient ready to interact with Azure ResourceManager.</returns>
    internal static ArmClient BuildArmClient(DatahubPortalConfiguration portalConfiguration)
    {
        var credential = new ClientSecretCredential(portalConfiguration.AzureAd.TenantId,
            portalConfiguration.AzureAd.InfraClientId, portalConfiguration.AzureAd.InfraClientSecret);
        var armOptions = new ArmClientOptions
        {
            Retry = { MaxRetries = 5, MaxDelay = TimeSpan.FromSeconds(5), Mode = RetryMode.Exponential }
        };
        var armClient = new ArmClient(credential, portalConfiguration.AzureAd.SubscriptionId, armOptions);
        return armClient;
    }

    /// <summary>
    /// Refreshes the current state of the storage account's AllowSharedKeyAccess setting.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RefreshStorageAllowSharedKeyAccess()
    {
        _isLoadingAccessState = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var armClient = BuildArmClient(_portalConfiguration);
            var resourceId = await BuildStorageResourceIdentifier();
            _accessState = await FetchStorageAllowSharedKeyAccess(armClient, resourceId);
        }
        catch (InvalidOperationException e)
        {
            _doesNotExist = true;
        }

        _isLoadingAccessState = false;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Fetches the status of whether shared key access is allowed for a given storage account.
    /// </summary>
    /// <param name="armClient">An instance of the ArmClient used to interact with Azure ResourceManager.</param>
    /// <param name="storageResourceIdentifier">The resource identifier for the storage account.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether shared key access is allowed, or null if the information is unavailable.</returns>
    internal static async Task<bool?> FetchStorageAllowSharedKeyAccess(ArmClient armClient,
        ResourceIdentifier storageResourceIdentifier)
    {
        var storage = armClient.GetStorageAccountResource(storageResourceIdentifier);
        var response = await storage.GetAsync();
        return response.Value.Data.AllowSharedKeyAccess;
    }

    /// <summary>
    /// Asynchronously constructs a ResourceIdentifier for a storage resource.
    /// </summary>
    /// <returns>A Task that represents the asynchronous operation, containing the ResourceIdentifier for the storage resource.</returns>
    private async Task<ResourceIdentifier> BuildStorageResourceIdentifier()
    {
        if (!string.IsNullOrWhiteSpace(_storageResourceId))
        {
            return new ResourceIdentifier(_storageResourceId);
        }

        _storageResourceId = await LoadStorageResourceId();
        return new ResourceIdentifier(_storageResourceId);
    }

    /// <summary>
    /// Loads the storage resource ID for a specific workspace asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result contains the storage resource ID as a string.</returns>
    private async Task<string> LoadStorageResourceId()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await LoadStorageResourceId(context, WorkspaceAcronym);
    }

    /// <summary>
    /// Asynchronously loads the resource ID of the storage account associated with the given workspace acronym.
    /// </summary>
    /// <param name="context">The database context used to query project information.</param>
    /// <param name="workspaceAcronym">The acronym of the workspace whose storage resource ID is to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the storage resource ID as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the storage account name or resource group name is missing.</exception>
    internal static async Task<string> LoadStorageResourceId(DatahubProjectDBContext context, string workspaceAcronym)
    {
        var workspace = await context.Projects
            .AsNoTracking()
            .Include(w => w.Resources)
            .Include(w => w.DatahubAzureSubscription)
            .FirstAsync(w => w.Project_Acronym_CD == workspaceAcronym);

        var workspaceSubscriptionId = workspace.DatahubAzureSubscription.SubscriptionId;
        var workspaceResourceGroup = TerraformVariableExtraction.ExtractWorkspaceResourceGroupName(workspace);
        var storageAccountName = TerraformVariableExtraction.ExtractAzureStorageAccountName(workspace);

        if (string.IsNullOrEmpty(storageAccountName) || string.IsNullOrEmpty(workspaceResourceGroup))
        {
            throw new InvalidOperationException("Storage account name or resource group name is missing.");
        }

        var result =
            $"/subscriptions/{workspaceSubscriptionId}/resourceGroups/{workspaceResourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}";

        return result;
    }


    /// <summary>
    /// Toggles the shared key access setting for a storage account associated with a workspace.
    /// Updates the internal state and notifies the user of the success or failure of the operation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private async Task ToggleStorageAllowSharedKeyAccess()
    {
        _isLoadingAccessState = true;
        await InvokeAsync(StateHasChanged);

        _snackbar.Add(Localizer["Updating shared key access..."], Severity.Info);
        await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserToggleStorageAllowSharedKeyAccess);

        var armClient = BuildArmClient(_portalConfiguration);
        var resourceId = await BuildStorageResourceIdentifier();
        var storage = armClient.GetStorageAccountResource(resourceId);
        var response = await storage.UpdateAsync(new StorageAccountPatch()
        {
            AllowSharedKeyAccess = _accessState != true
        });

        _accessState = response.Value.Data.AllowSharedKeyAccess;
        _snackbar.Add(Localizer["Shared key access updated successfully."], Severity.Success);

        _isLoadingAccessState = false;
        await InvokeAsync(StateHasChanged);
    }
}