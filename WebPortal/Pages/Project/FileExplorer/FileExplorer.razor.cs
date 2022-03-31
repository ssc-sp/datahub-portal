using Datahub.Core.Data;
using Datahub.Core.Services;
using Datahub.Portal.Services;
using Datahub.Portal.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Project.FileExplorer;

public partial class FileExplorer
{
    private async Task FetchStorageBlobsPageAsync()
    {
        _loading = true;
        StateHasChanged();

        var (folders, files, continuationToken) =
            await _dataRetrievalService.GetStorageBlobPagesAsync(ProjectAcronym, ContainerName, _user, _currentFolder, _continuationToken);

        _continuationToken = continuationToken;
        _files = files;
        _folders = folders;

        _loading = false;
        StateHasChanged();
    }
    
    private void HandleNewFolder(string newFolderName)
    {
        if (_folders.Contains(newFolderName))
            return;

        newFolderName = newFolderName
            .Replace("/", "")
            .Trim();

        _folders.Add($"{_currentFolder}{newFolderName}/");
    }
    
    private async Task HandleFileDelete(string filename)
    {
        var selectedFile = _files?.FirstOrDefault(f => f.name == filename);
        var success = await _dataRemovalService.DeleteStorageBlob(selectedFile, ProjectAcronym, ContainerName, _user);
        if (success)
        {
            _files?.RemoveAll(f => f.name.Equals(filename, StringComparison.OrdinalIgnoreCase));
        }
        _selectedItem = _currentFolder;
        
    }

    private async Task HandleFileItemDrop(string folder, string filename)
    {
        if (!string.IsNullOrWhiteSpace(folder) && !string.IsNullOrWhiteSpace(filename))
        {
            var oldFilename = (_currentFolder + filename).TrimStart('/');
            var newFilename = (folder + filename).TrimStart('/');
            
            await _dataUpdatingService.RenameStorageBlob(oldFilename, newFilename, ProjectAcronym, ContainerName);
            _files.RemoveAll(f => f.name == oldFilename);
        }
    }
    
    private async Task HandleFileRename(string fileRename)
    {
        if (!string.IsNullOrWhiteSpace(fileRename))
        {
            var oldFilename = (_currentFolder + GetFileName(_selectedItem)).TrimStart('/');
            var newFilename = (_currentFolder + fileRename).TrimStart('/');
            
            await _dataUpdatingService.RenameStorageBlob(oldFilename, newFilename, ProjectAcronym, ContainerName);
            var file = _files.First(f => f.name == oldFilename);
            file.name = newFilename;
        }
    }
    
    private async Task UploadFile(IBrowserFile browserFile, string folder)
    {
        if (browserFile == null)
            return;

        var newFilename = (folder + browserFile.Name).TrimStart('/');

        if (_files.Any(f => f.name == newFilename))
        {
            var response = await _jsRuntime.InvokeAsync<bool>("confirm",
                string.Format(Localizer["File '{0}' already exists. Do you want to overwrite it?"], newFilename));
            if (!response)
                return;
        }

        var fileMetadata = new FileMetaData
        {
            folderpath = folder,
            filename = (folder + browserFile.Name).TrimStart('/'),
            filesize = browserFile.Size.ToString(),
            uploadStatus = FileUploadStatus.SelectedToUpload,
            BrowserFile = browserFile
        };

        await _apiService.PopulateOtherMetadata(fileMetadata);
        _uploadingFiles.Add(fileMetadata);

        _ = InvokeAsync(async () =>
        {
            await _apiService.UploadGen2File(fileMetadata, ProjectAcronym.ToLower(), ContainerName, (uploadedBytes) =>
            {
                fileMetadata.uploadedBytes = uploadedBytes;
                StateHasChanged();
            });

            _uploadingFiles.Remove(fileMetadata);
            if (folder == _currentFolder)
            {
                _files.RemoveAll(f => f.name == fileMetadata.name);
                _files.Add(fileMetadata);
            }
            StateHasChanged();
        });

        StateHasChanged();
    }

    private async Task HandleFileDownload(string filename)
    {   
        var selectedFile = _files?.FirstOrDefault(f => f.name == filename);
        var uri = await _dataRetrievalService.DownloadFile(ContainerName, selectedFile, ProjectAcronym);
        await _jsRuntime.InvokeVoidAsync("open", uri.ToString(), "_blank");
    }
    
    
    private string GetDirectoryName(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.Contains("/"))
            return string.Empty;

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
        _selectedItem = folderName;
        await FetchStorageBlobsPageAsync();
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
    
    private async void HandleSearch(string newValue, KeyboardEventArgs ev)
    {
        await _searchThrottler.SetQuery(newValue);
    }
    
    private void ResetSearch()
    {
        _lastSearch = null;
        _searchResults = _files;
    }

    private async Task ThrottleSearch(string searchText)
    {
        if (_lastSearch != searchText)
        {
            _searchResults.Clear();
            _lastSearch = searchText;
            _selectedItem = string.Empty;
            try
            {
                var all = string.IsNullOrEmpty(searchText);
                _searchResults = new List<FileMetaData>(_files.Where(c => all || c.name.Contains(searchText ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)));
            }
            finally
            {
                await InvokeAsync(StateHasChanged);
            }
        }            
    }
}