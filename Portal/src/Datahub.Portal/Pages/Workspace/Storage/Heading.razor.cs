using Datahub.Core.Model.Achievements;
using Datahub.Infrastructure.Services.Storage;
using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Workspace.Storage;

public partial class Heading
{
    private enum ButtonAction
    {
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
    
    private async Task HandleUpload()
    {
        if (IsActionDisabled(ButtonAction.Upload))
            return;

        await _module.InvokeVoidAsync("promptForFileUpload");
    }

    private async Task HandleDownload()
    {
        if (IsActionDisabled(ButtonAction.Download))
            return;

        var downloads = SelectedItems
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false);

        foreach (var download in downloads)
        {
            await OnFileDownload.InvokeAsync(download);
            await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserDownloadFile);
        }
    }

    private async Task HandlePublish()
    {
        if (IsActionDisabled(ButtonAction.Publish)) return;

        var publishFiles = SelectedItems
            .Select(sel => Files?.FirstOrDefault(f => f.name == sel))
            .Where(f => f is not null);

        await OnPublishFiles.InvokeAsync(publishFiles);
        //TODO telemetry
    }

    private async Task HandleAzSyncDown()
    {
        var uri = await _dataRetrievalService.GenerateSasToken(DataRetrievalService.DEFAULT_CONTAINER_NAME, ProjectAcronym, 14);
        await _module.InvokeAsync<string>("azSyncDown", uri.ToString(), _dotNetHelper);
    }

    private async Task HandleShare()
    {
        await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserShareFile);

        var selectedFile = _selectedFiles.FirstOrDefault();
        if (selectedFile is null)
            return;

        var sb = new System.Text.StringBuilder();
        sb.Append("/sharingworkflow/");
        sb.Append(selectedFile.fileid);
        sb.Append("?filename=");
        sb.Append(selectedFile.filename);
        if (!string.IsNullOrWhiteSpace(ProjectAcronym))
        {
            sb.Append("&project=");
            sb.Append(ProjectAcronym);
        }
        else
        {
            sb.Append("&folderpath=");
            sb.Append(selectedFile.folderpath);
        }
        _navigationManager.NavigateTo(sb.ToString());
    }

    private async Task HandleDelete()
    {
        if (IsActionDisabled(ButtonAction.Delete))
            return;

        var deletes = SelectedItems
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false);

        foreach (var delete in deletes)
        {
            await OnFileDelete.InvokeAsync(delete);
        }
    }

    private async Task HandleRename()
    {
        if (IsActionDisabled(ButtonAction.Rename))
            return;
        
        var selectedFile = _selectedFiles.FirstOrDefault();
        if (selectedFile is not null && _ownsSelectedFiles)
        {
            var newName = await _jsRuntime.InvokeAsync<string>("prompt", "Enter new name", 
                FileExplorer.GetFileName(selectedFile.filename));
            newName = newName?.Replace("/", "").Trim();

            await OnFileRename.InvokeAsync(newName);
        }
    }

    private async Task HandleNewFolder()
    {
        if (IsActionDisabled(ButtonAction.NewFolder))
            return;
        
        var newFolderName = await _module.InvokeAsync<string>("promptForNewFolderName");
        if (!string.IsNullOrWhiteSpace(newFolderName))
        {
            await OnNewFolder.InvokeAsync(newFolderName.Trim());
        }
    }

    private async Task HandleDeleteFolder()
    {
        if (IsActionDisabled(ButtonAction.DeleteFolder))
            return;
        
        await OnDeleteFolder.InvokeAsync();
    }

    private bool IsActionDisabled(ButtonAction buttonAction)
    {
        if (_currentUserRole is null)
            return true;
        
        return buttonAction switch
        {
            ButtonAction.Upload   => !_currentUserRole.IsAtLeastCollaborator,
            ButtonAction.AzSync   => !_isElectron,
            ButtonAction.Download => _selectedFiles is null || !_selectedFiles.Any() || !_currentUserRole.IsAtLeastGuest,
            ButtonAction.Share    => !_isUnclassifiedSingleFile,
            ButtonAction.Delete   => _selectedFiles is null || !_selectedFiles.Any() || !_currentUserRole.IsAtLeastCollaborator,
            ButtonAction.Rename   => _selectedFiles is null || !_selectedFiles.Any() || !_currentUserRole.IsAtLeastCollaborator || SelectedItems.Count > 1,
            ButtonAction.NewFolder => !_currentUserRole.IsAtLeastCollaborator,
            ButtonAction.DeleteFolder => Files.Any() || Folders.Any() || CurrentFolder == "/" || !_currentUserRole.IsAtLeastCollaborator,
            ButtonAction.Publish => !_config.CkanConfiguration.IsFeatureEnabled || _selectedFiles is null || !_selectedFiles.Any() || !_currentUserRole.IsAtLeastCollaborator,
            _ => false
        };
    }
}
