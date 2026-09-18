using Azure.Storage.Blobs.Models;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.JSInterop;
using Microsoft.TeamFoundation.Common;
using MudBlazor;
using System.Timers;

namespace Datahub.Portal.Pages.Workspace.Storage;

public partial class StorageHeading
{
    private ICloudStorageManager? _tierManager;
    private string? _tierContainer;
    private bool _changingTier;
    private string? _tierStatus;
    private string? _tierError;

    private void RefreshStorageTierOptions()
    {
        if (ReferenceEquals(_tierManager, StorageManager) && _tierContainer == ContainerName)
            return;
        _tierManager = StorageManager;
        _tierContainer = ContainerName;
        _storageTiers = StorageManager.GetFileStorageTiersList();
        SelectedStorageTier = "";
        _tierStatus = null;
        _tierError = null;
    }

    private enum ButtonAction
    {
        BackToContainers,
        Upload,
        Download,
        Share,
        Delete,
        Rename,
        AzSync,
        DeleteFolder,
        NewFolder,
        TierChange,
        Publish
    }

    private string ButtonActionToString(ButtonAction action)
    {
        switch (action)
        {
            case ButtonAction.BackToContainers:
                return "Back to Containers";
            case ButtonAction.DeleteFolder:
                return "Delete Folder";
            case ButtonAction.NewFolder:
                return "New Folder";
            default:
                return action.ToString();
        }
    }

    private string GetButtonActionIcon(ButtonAction action)
    {
        switch (action)
        {
            case ButtonAction.BackToContainers:
                return "fas fa-arrow-left";
            case ButtonAction.Upload:
                return "fas fa-upload";
            case ButtonAction.Download:
                return "fas fa-download";
            case ButtonAction.Rename:
                return "fas fa-edit";
            case ButtonAction.Delete:
                return "fas fa-trash-alt";
            case ButtonAction.NewFolder:
                return "fas fa-folder-plus";
            case ButtonAction.DeleteFolder:
                return "fas fa-folder-minus";
            case ButtonAction.TierChange:
                return Icons.Material.Filled.Storage;
            case ButtonAction.Publish:
                return "fas fa-bullhorn";
            default:
                return "fas fa-arrow-left";
        }
    }

    private async Task GetButtonActionHandler(ButtonAction action)
    {
        switch(action)
        {
            case ButtonAction.BackToContainers:
                await HandleBackToContainers();
                break;
            case ButtonAction.Upload:
                await HandleUpload();
                break;
            case ButtonAction.Download:
                await HandleDownload();
                break;
            case ButtonAction.Rename:
                await HandleRename();
                break;
            case ButtonAction.Delete:
                await HandleDelete();
                break;
            case ButtonAction.NewFolder:
                await HandleNewFolder();
                break;
            case ButtonAction.DeleteFolder:
                await HandleDeleteFolder();
                break;
            case ButtonAction.Publish:
                await HandlePublish();
                break;
            default:
                break;
        }
    }
    
    private async Task HandleUpload()
    {
        if (await IsActionDisabled(ButtonAction.Upload))
            return;

        await _module.InvokeVoidAsync("promptForFileUpload");
    }

    private async Task HandleBackToContainers()
    {
        await OnBackToContainers.InvokeAsync();
    }

    private async Task HandleDownload()
    {
        if (await IsActionDisabled(ButtonAction.Download))
            return;

        List<string> tiers = StorageManager.ProviderType == CloudStorageProviderType.AWS
            ? ["STANDARD_IA", "ONEZONE_IA", "GLACIER_IR"]
            : [AccessTier.Cool.ToString(), AccessTier.Cold.ToString()];

        var downloads = SelectedItems?
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false);

        var downloadPaths = (_selectedFiles ?? []).Select(file =>
            $"{CurrentFolder?.TrimEnd('/')}/{file.name.TrimStart('/')}".TrimStart('/')).ToList();
        if (await CheckIfAnyFilesInTiers(downloadPaths, tiers, StorageManager, ContainerName))
        {
            bool confirm = await _module.InvokeAsync<bool>("confirmDownloadCoolOrCold", Localizer["Are you sure you want to download these files? There is increased cost to download this storage type."].ToString());

            if (!confirm) return;
        }
        
        if (downloads is null)
            return;

        foreach (var download in downloads)
        {
            await OnFileDownload.InvokeAsync(download);
            await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserDownloadFile);
        }
    }

    private async Task HandlePublish()
    {
        if (await IsActionDisabled(ButtonAction.Publish)) return;

        if (_isPublishingBlockedForWorkspace)
        {
            await ShowPublishingBlockedDialog();
            return;
        }

        var publishFiles = SelectedItems?
            .Select(sel => Files?.FirstOrDefault(f => f.name == sel))
            .Where(f => f is not null)
            .Select(f => f!);

        if (publishFiles is null)
            return;

        await OnPublishFiles.InvokeAsync(publishFiles);
        //TODO telemetry
    }
    private async Task HandleDelete()
    {
        if (await IsActionDisabled(ButtonAction.Delete))
            return;

        var deletes = SelectedItems?
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false);

        if (deletes is null)
            return;

        foreach (var delete in deletes)
        {
            await OnFileDelete.InvokeAsync(delete);
        }
    }

    private async Task HandleRename()
    {
        if (await IsActionDisabled(ButtonAction.Rename))
            return;
        
        var selectedFile = _selectedFiles?.FirstOrDefault();
        if (selectedFile is not null && _ownsSelectedFiles)
        {
            var newName = await _jsRuntime.InvokeAsync<string>("prompt", Localizer["Enter a new name for the file."].ToString(), 
                FileExplorer.GetFileName(selectedFile.filename ?? string.Empty));
            newName = newName?.Replace("/", "").Trim();

            await OnFileRename.InvokeAsync(newName);
        }
    }

    private async Task HandleNewFolder()
    {
        if (await IsActionDisabled(ButtonAction.NewFolder))
            return;
        
        var newFolderName = await _module.InvokeAsync<string>("promptForNewFolderName", Localizer["Enter a new name for the folder."].ToString());
        if (!string.IsNullOrWhiteSpace(newFolderName))
        {
            await OnNewFolder.InvokeAsync(newFolderName.Trim());
        }
    }
    private async Task HandleDeleteFolder()
    {
        if (await IsActionDisabled(ButtonAction.DeleteFolder))
            return;

        var folderName = SelectedItems?.FirstOrDefault();
        if (folderName is null)
        {
            folderName = CurrentFolder;
        }
        if (folderName.Length < CurrentFolder.Length)  // delete from inside folder
        {
            await OnDeleteFolder.InvokeAsync(CurrentFolder);
            return;
        }
        
        if (folderName != "/")
        {
            await OnDeleteFolder.InvokeAsync(folderName);
        }
    }

    private bool CanDeleteCurrentFolder()
    {
        var folderName = SelectedItems?.FirstOrDefault();
        if (folderName is null)
        {
            folderName = CurrentFolder;
        }
        if (folderName.Length < CurrentFolder.Length)
        {
            return !Files.Any() && !Folders.Any();
        }
        return CanDeleteFolder(folderName);
    }

    /// <summary>
    /// Handler for a new tier being selected. Iterates through selected items and updates their tier.
    /// </summary>
    /// <param name="newTier">New tier to be set</param>
    /// <returns></returns>
    private async Task HandleTierChange(string newTier)
    {
        if (newTier.IsNullOrEmpty() || _changingTier
            || await IsActionDisabled(ButtonAction.TierChange))
        {
            return; // Value was cleared
        }

        if (!StorageManager.GetFileStorageTiersList().Contains(newTier, StringComparer.Ordinal))
            return;

        _changingTier = true;
        try
        {
            if (newTier == AccessTier.Archive.ToString() ||
                (StorageManager.ProviderType == CloudStorageProviderType.AWS && newTier is "GLACIER" or "DEEP_ARCHIVE"))
            {
                var message = StorageManager.ProviderType == CloudStorageProviderType.AWS
                    ? Localizer["Are you sure you want to change the file(s) to archive tier? To access them again, restore them using AWS tooling. This can take several hours."]
                    : Localizer["Are you sure you want to change the file(s) to archive tier? If you need to access them, it will take time to re-hydrate."];
                bool confirm = await _module.InvokeAsync<bool>("confirmStorageTierChange", message.ToString());
                if (!confirm) return;
            }
            await ApplyTierChangeAsync(newTier);
        }
        finally { _changingTier = false; }
    }

    private async Task ApplyTierChangeAsync(string newTier)
    {
        var manager = StorageManager;
        var container = ContainerName;
        var paths = SelectedItems?
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false)
            .Select(file => $"{CurrentFolder?.TrimEnd('/')}/{file.TrimStart('/')}".TrimStart('/'))
            .ToList() ?? [];
        if (paths.Count == 0) return;
        _tierError = null;
        _tierStatus = Localizer["Changing storage tier…"];
        var errors = new List<string>();

        foreach (var path in paths)
        {
            try
            {
                if (!await manager.SetFileStorageTierAsync(container, path, newTier))
                    errors.Add(Localizer["Failed to change storage tier"]);
            }
            catch (StorageTierChangeException ex) { errors.Add(Localizer[ex.Message]); }
            catch { errors.Add(Localizer["Failed to change storage tier"]); }
        }

        if (!ReferenceEquals(manager, StorageManager) || container != ContainerName)
            return;
        _tierStatus = null;
        if (errors.Count == 0)
        {
            var label = Localizer[AWSCloudStorageManager.GetStorageClassLabel(newTier)].ToString();
            _tierStatus = Localizer["Storage tier changed to {0} successfully", label];
            _snackbar.Add(Localizer["Storage tier changed to {0} successfully", label], Severity.Success);
        }
        else
        {
            _tierError = string.Join(" ", errors.Distinct());
            _snackbar.Add(Localizer["Failed to change storage tier"], Severity.Error);
        }

        if (ReferenceEquals(manager, StorageManager) && container == ContainerName && OnStorageTierChanged.HasDelegate)
            await OnStorageTierChanged.InvokeAsync(newTier);
    }

    private async Task<bool> AnySelectedFileUnavailableAsync()
    {
        if (StorageManager.ProviderType == CloudStorageProviderType.AWS)
        {
            foreach (var file in _selectedFiles ?? [])
            {
                var path = $"{CurrentFolder?.TrimEnd('/')}/{file.name.TrimStart('/')}".TrimStart('/');
                if ((await StorageManager.GetFileArchiveStatusAsync(ContainerName, path)).State != CloudStorageArchiveState.Available)
                    return true;
            }
            return false;
        }
        var paths = (_selectedFiles ?? []).Select(file =>
            $"{CurrentFolder?.TrimEnd('/')}/{file.name.TrimStart('/')}".TrimStart('/')).ToList();
        return await CheckIfAnyFilesInTiers(paths, [AccessTier.Archive.ToString()], StorageManager, ContainerName);
    }

    private async Task<bool> CheckIfAnyFilesInTiers(List<PortalFileMetadata> selectedFiles, List<string> checkTiers)
    {
        bool result = await CheckIfAnyFilesInTiers(selectedFiles, checkTiers, StorageManager, ContainerName);
        return result;
    }

    public static async Task<bool> CheckIfAnyFilesInTiers(List<string> selectedFiles, List<string> checkTiers, ICloudStorageManager storageManager, string containerName)
    {
        foreach (var file in selectedFiles)
        {
            var fileTier = await storageManager.GetFileStorageTierAsync(containerName, file);

            foreach (var tier in checkTiers)
            {
                if (fileTier == tier)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static async Task<bool> CheckIfAnyFilesInTiers(List<PortalFileMetadata> selectedFiles, List<string> checkTiers, ICloudStorageManager storageManager, string containerName)
    {
        List<string> filePaths = new List<string>();
        foreach (var file in selectedFiles)
        {
            filePaths.Add(file.fullPathFromRoot);
        }
        bool result = await CheckIfAnyFilesInTiers(filePaths, checkTiers, storageManager, containerName);
        return result;
    }

    private async Task<bool> IsActionDisabled(ButtonAction buttonAction)
    {
        if(buttonAction is ButtonAction.BackToContainers)
        { return false; }

        if (_currentUserRole is null)
            return true;

        if (Readonly && buttonAction is ButtonAction.Upload or ButtonAction.Delete or ButtonAction.Rename or ButtonAction.NewFolder or ButtonAction.DeleteFolder or ButtonAction.TierChange)
            return true;

        var hasExternalStorageAccess = _currentUserRole.Id is (int)Project_Role.RoleNames.Storage or (int)Project_Role.RoleNames.WebAppAndStorage;
        var canWriteStorage = _currentUserRole.IsAtLeastCollaborator || hasExternalStorageAccess;
        var canReadStorage = _currentUserRole.IsAtLeastGuest || hasExternalStorageAccess;

        return buttonAction switch
        {
            ButtonAction.Upload => !canWriteStorage,
            ButtonAction.AzSync => !_isElectron,
            ButtonAction.Download => _selectedFiles is null || !_selectedFiles.Any() || !canReadStorage || await AnySelectedFileUnavailableAsync(),
            ButtonAction.Share => !_isUnclassifiedSingleFile || await AnySelectedFileUnavailableAsync(),
            ButtonAction.Delete => _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage,
            ButtonAction.Rename => _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage || SelectedItems.Count > 1 || await AnySelectedFileUnavailableAsync(),
            ButtonAction.NewFolder => !canWriteStorage,
            ButtonAction.DeleteFolder => !CanDeleteCurrentFolder() || !canWriteStorage,
            ButtonAction.TierChange => _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage || IsStorageTierDisabled,
            ButtonAction.Publish => !_config.CkanConfiguration.IsFeatureEnabled || _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage || await AnySelectedFileUnavailableAsync(),
            _ => false
        };
    }
}
