using Datahub.Achievements.Models;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Project.FileExplorer;

public partial class FileExplorer
{
    private async Task RefreshStoragePageAsync()
    {
        _loading = true;
        StateHasChanged();

        var (folders, files, continuationToken) = await _projectDataRetrievalService.GetDfsPagesAsync(ProjectAcronym, 
            ContainerName, _currentFolder, _continuationToken);

        _continuationToken = continuationToken;
        _files = files;
        _folders = folders;

        _loading = false;
        StateHasChanged();
    }

    private async Task HandleNewFolder(string folderName)
    {
        var newFolderName = folderName.Replace("/", "").Replace("\\", "").Trim();
        var newFolderPath = JoinPath(_currentFolder, newFolderName);
        if (_folders.Contains(newFolderPath))
            return;

        if (!await _projectDataRetrievalService.CreateFolderAsync(ProjectAcronym, ContainerName, _currentFolder, newFolderName))
            return;

        _folders.Add(newFolderPath);

        await _achievementService.AddOrIncrementTelemetryEvent(DatahubUserTelemetry.TelemetryEvents.UserCreateFolder);
    }

    private async Task HandleFileDelete(string fileName)
    {
        var message = string.Format(Localizer["Are you sure you want to delete file \"{0}\"?"], fileName);
        if (!await _jsRuntime.InvokeAsync<bool>("confirm", message))
            return;

        if (!await _projectDataRetrievalService.DeleteFileAsync(ProjectAcronym, ContainerName, JoinPath(_currentFolder, fileName)))
            return;

        _files?.RemoveAll(f => f.name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

        _selectedItems = new HashSet<string> {_currentFolder};
        await _achievementService.AddOrIncrementTelemetryEvent(DatahubUserTelemetry.TelemetryEvents.UserDeleteFile);
    }

    private async Task HandleFileItemDrop(string folder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName))
            return;

        var oldFileName = JoinPath(_currentFolder, fileName);
        var newFileName = JoinPath(folder, fileName);

        if (await PreventOverwrite(newFileName))
            return;

        if (!await _projectDataRetrievalService.RenameFileAsync(ProjectAcronym, ContainerName, oldFileName, newFileName))
            return;
        
        _files.RemoveAll(f => f.name == fileName);
    }

    private async Task HandleFileRename(string fileRename)
    {
        if (string.IsNullOrWhiteSpace(fileRename))
            return;
        
        var currentFileName = GetFileName(_selectedItem);

        var oldFileName = JoinPath(_currentFolder, currentFileName);
        var newFileName = JoinPath(_currentFolder, fileRename);

        if (await PreventOverwrite(newFileName))
            return;

        if (!await _projectDataRetrievalService.RenameFileAsync(ProjectAcronym, ContainerName, oldFileName, newFileName))
            return;

        var targetFile = _files.FirstOrDefault(f => f.name == currentFileName);
        if (targetFile is not null)
            targetFile.name = fileRename;
    }

    private async Task HandleDeleteFolder()
    {
        var message = string.Format(Localizer["Are you sure you want to delete folder \"{0}\"?"], GetDirectoryName(_currentFolder));
        if (!await _jsRuntime.InvokeAsync<bool>("confirm", message))
            return;

        if (!await _projectDataRetrievalService.DeleteFolderAsync(ProjectAcronym, ContainerName, _currentFolder))
            return;

        await SetCurrentFolder(GetDirectoryName(_currentFolder));
    }

    private async Task<bool> PreventOverwrite(string filePath)
    {
        var fileExists = await _projectDataRetrievalService.FileExistsAsync(ProjectAcronym, ContainerName, filePath);
        
        if (!fileExists)
            return false;

        return !await _jsRuntime.InvokeAsync<bool>("confirm",
            string.Format(Localizer["File '{0}' already exists. Do you want to overwrite it?"], filePath));
    }

    private string JoinPath(string folder, string fileName)
    {
        var splitPath = (folder ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        splitPath.Add(fileName);
        return string.Join("/", splitPath);
    }

    private async Task UploadFile(IBrowserFile browserFile, string folder)
    {
        if (browserFile == null)
            return;

        var newFilePath = JoinPath(folder, browserFile.Name);
        if (await PreventOverwrite(newFilePath))
            return;

        var fileMetadata = new FileMetaData
        {
            id = Guid.NewGuid().ToString(),
            createdby = GraphUser.Mail,
            folderpath = folder,
            filename = browserFile.Name,
            filesize = browserFile.Size.ToString(),
            uploadStatus = FileUploadStatus.SelectedToUpload,
            BrowserFile = browserFile
        };

        _uploadingFiles.Add(fileMetadata);

        _ = InvokeAsync(async () =>
        {
            var succeeded = await _projectDataRetrievalService.UploadFileAsync(ProjectAcronym.ToLower(), ContainerName, fileMetadata, uploadedBytes =>
            {
                fileMetadata.uploadedBytes = uploadedBytes;
                _ = InvokeAsync(StateHasChanged);
            });

            _uploadingFiles.Remove(fileMetadata);
            if (folder == _currentFolder)
            {
                _files.RemoveAll(f => f.name == fileMetadata.name);
                if (succeeded)
                {
                    _files.Add(fileMetadata);
                }                    
            }

            await _achievementService.AddOrIncrementTelemetryEvent(DatahubUserTelemetry.TelemetryEvents.UserUploadFile);
            StateHasChanged();
        });

        StateHasChanged();
    }

    private async Task HandleFileDownload(string filename)
    {   
        var uri = await _projectDataRetrievalService.DownloadFileAsync(ProjectAcronym, ContainerName, JoinPath(_currentFolder, filename));
        await _module.InvokeVoidAsync("downloadFile", uri.ToString());
    }
    
    private string GetDirectoryName(string path)
    {
        var lastIndex = path.TrimEnd('/').LastIndexOf("/", StringComparison.Ordinal);
        return lastIndex == -1 ? "/" : path[..lastIndex] + "/";
    }

    public static string GetFileName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var lastIndex = path.TrimEnd('/').LastIndexOf("/", StringComparison.Ordinal);
        return lastIndex == -1 ? path : path[(lastIndex + 1)..];
    }

    private async Task SetCurrentFolder(string folderName)
    {
        _currentFolder = folderName;
        _selectedItems = new HashSet<string> { folderName };
        await RefreshStoragePageAsync();
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        await UploadFiles(e, _currentFolder);
    }

    private async Task UploadFiles(InputFileChangeEventArgs e, string folderName)
    {
        foreach (var browserFile in e.GetMultipleFiles())
        {
            await UploadFile(browserFile, folderName);
        }
    }

    private void HandleSearch(string newValue, KeyboardEventArgs ev)
    {
        _filterValue = newValue.Trim();
        StateHasChanged();
    }

    private void ResetSearch()
    {
        _filterValue = string.Empty;
    }
    
    private void HandleFileSelectionClick(string filename)
    {
        _selectedItems.RemoveWhere(i => i.EndsWith("/", StringComparison.InvariantCulture));
        
        if (_selectedItems.Contains(filename))
        {
            _selectedItems.Remove(filename);
        }
        else
        {
            _selectedItems.Add(filename);
        }
    }
}