using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Workspace.Storage;

public partial class StorageHeading
{
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
        if (IsActionDisabled(ButtonAction.Upload))
            return;

        await _module.InvokeVoidAsync("promptForFileUpload");
    }

    private async Task HandleBackToContainers()
    {
        await OnBackToContainers.InvokeAsync();
    }

    private async Task HandleDownload()
    {
        if (IsActionDisabled(ButtonAction.Download))
            return;

        var downloads = SelectedItems?
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false);
        
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
        if (IsActionDisabled(ButtonAction.Publish)) return;

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
        if (IsActionDisabled(ButtonAction.Delete))
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
        if (IsActionDisabled(ButtonAction.Rename))
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
        if (IsActionDisabled(ButtonAction.NewFolder))
            return;
        
        var newFolderName = await _module.InvokeAsync<string>("promptForNewFolderName", Localizer["Enter a new name for the folder."].ToString());
        if (!string.IsNullOrWhiteSpace(newFolderName))
        {
            await OnNewFolder.InvokeAsync(newFolderName.Trim());
        }
    }
    private async Task HandleDeleteFolder()
    {
        if (IsActionDisabled(ButtonAction.DeleteFolder))
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


    private bool IsActionDisabled(ButtonAction buttonAction)
    {
        if(buttonAction is ButtonAction.BackToContainers)
        { return false; }

        if (_currentUserRole is null)
            return true;

        if (Readonly && buttonAction is ButtonAction.Upload or ButtonAction.Delete or ButtonAction.Rename or ButtonAction.NewFolder or ButtonAction.DeleteFolder)
            return true;

        var hasExternalStorageAccess = _currentUserRole.Id is (int)Project_Role.RoleNames.Storage or (int)Project_Role.RoleNames.WebAppAndStorage;
        var canWriteStorage = _currentUserRole.IsAtLeastCollaborator || hasExternalStorageAccess;
        var canReadStorage = _currentUserRole.IsAtLeastGuest || hasExternalStorageAccess;

        return buttonAction switch
        {
            ButtonAction.Upload => !canWriteStorage,
            ButtonAction.AzSync => !_isElectron,
            ButtonAction.Download => _selectedFiles is null || !_selectedFiles.Any() || !canReadStorage,
            ButtonAction.Share => !_isUnclassifiedSingleFile,
            ButtonAction.Delete => _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage,
            ButtonAction.Rename => _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage || SelectedItems.Count > 1,
            ButtonAction.NewFolder => !canWriteStorage,
            ButtonAction.DeleteFolder => !CanDeleteCurrentFolder() || !canWriteStorage,
            ButtonAction.Publish => !_config.CkanConfiguration.IsFeatureEnabled || _selectedFiles is null || !_selectedFiles.Any() || !canWriteStorage,
            _ => false
        };
    }
}
