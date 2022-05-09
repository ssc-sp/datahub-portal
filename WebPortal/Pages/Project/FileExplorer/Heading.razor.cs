using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Project.FileExplorer;

public partial class Heading
{
    private async Task BreadcrumbClicked(string breadcrumb)
    {
        var index = CurrentFolder.IndexOf(breadcrumb, StringComparison.OrdinalIgnoreCase);
        await SetCurrentFolder.InvokeAsync(CurrentFolder[..(index + breadcrumb.Length)] + "/");
    }

    private async Task HandleDownload()
    {
        var downloads = SelectedItems
            .Where(selectedItem => Files?.Any(f => f.name == selectedItem) ?? false);

        foreach (var download in downloads)
        {
            await OnFileDownload.InvokeAsync(download);
        }
    }

    private void HandleShare()
    {
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

    private enum ButtonAction
    {
        Download,
        Share,
        Delete,
        Rename
    }

    private bool IsActionDisabled(ButtonAction buttonAction)
    {
        return buttonAction switch
        {
            ButtonAction.Download => _selectedFiles is null || !_selectedFiles.Any() || !_ownsSelectedFiles,
            ButtonAction.Share    => _selectedFiles is null || !_selectedFiles.Any() || !_ownsSelectedFiles || SelectedItems.Count > 1,
            ButtonAction.Delete   => _selectedFiles is null || !_selectedFiles.Any() || !_ownsSelectedFiles,
            ButtonAction.Rename   => _selectedFiles is null || !_selectedFiles.Any() || !_ownsSelectedFiles || SelectedItems.Count > 1,
            _ => false
        };
    }
}