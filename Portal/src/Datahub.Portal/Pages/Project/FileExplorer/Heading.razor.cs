using Datahub.Core.Model.Achievements;
using Datahub.Core.Services.Storage;
using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Project.FileExplorer;

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
        DeleteFolder
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
        if (!_ownsSelectedFiles)
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
        var newFolderName = await _module.InvokeAsync<string>("promptForNewFolderName");
        if (!string.IsNullOrWhiteSpace(newFolderName))
        {
            await OnNewFolder.InvokeAsync(newFolderName.Trim());
        }
    }

    private async Task HandleDeleteFolder()
    {
        await OnDeleteFolder.InvokeAsync();
    }

    private bool IsActionDisabled(ButtonAction buttonAction)
    {
        return buttonAction switch
        {
            ButtonAction.Upload   => !(_isDatahubAdmin || _isProjectMember),
            ButtonAction.AzSync   => !_isElectron,
            ButtonAction.Download => _selectedFiles is null || !_selectedFiles.Any() || !(_isDatahubAdmin || _isProjectMember),
            ButtonAction.Share    => !_isUnclassifiedSingleFile,
            ButtonAction.Delete   => _selectedFiles is null || !_selectedFiles.Any() || !_ownsSelectedFiles,
            ButtonAction.Rename   => _selectedFiles is null || !_selectedFiles.Any() || !_ownsSelectedFiles || SelectedItems.Count > 1,
            ButtonAction.DeleteFolder => Files.Any() || Folders.Any() || CurrentFolder == "/",
            _ => false
        };
    }
}
