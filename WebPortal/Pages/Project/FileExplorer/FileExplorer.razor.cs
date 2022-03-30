using Datahub.Core.Data;
using Datahub.Core.Services;
using Datahub.Portal.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Datahub.Portal.Pages.Project.FileExplorer;

public partial class FileExplorer
{

    [Inject] private IDataUpdatingService _dataUpdatingService { get; set; }

    
    private async Task FetchStorageBlobsPageAsync()
    {
        _loading = true;
        StateHasChanged();

        var (folders, files, continuationToken) =
            await _dataRetrievalService.GetStorageBlobPagesAsync(ProjectAcronym, DataRetrievalService.DEFAULT_CONTAINER_NAME, _user, _currentFolder, _continuationToken);

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
        _files.RemoveAll(f => f.name.Equals(filename, StringComparison.OrdinalIgnoreCase));
        await SetCurrentFolder(_currentFolder);
    }
    
    private async Task HandleFileRename(string newFilename)
    {
        if (!string.IsNullOrWhiteSpace(newFilename))
        {
            var oldFilename = _selectedItem;
            await _dataUpdatingService.RenameStorageBlob(oldFilename, newFilename, ProjectAcronym);
            var file = _files.First(f => f.name == oldFilename);
            file.name = newFilename;
        }
    }
    
    private async Task UploadFile(IBrowserFile browserFile)
    {
        if (browserFile == null)
            return;

        var newFilename = (_currentFolder + browserFile.Name).TrimStart('/');

        if (_files.Any(f => f.name == newFilename))
        {
            var response = await _jsRuntime.InvokeAsync<bool>("confirm",
                string.Format(Localizer["File '{0}' already exists. Do you want to overwrite it?"], newFilename));
            if (!response)
                return;
        }

        var fileMetadata = new FileMetaData
        {
            folderpath = _currentFolder,
            filename = (_currentFolder + browserFile.Name).TrimStart('/'),
            filesize = browserFile.Size.ToString(),
            uploadStatus = FileUploadStatus.SelectedToUpload,
            BrowserFile = browserFile
        };

        await _apiService.PopulateOtherMetadata(fileMetadata);
        _uploadingFiles.Add(fileMetadata);

        _ = InvokeAsync(async () =>
        {
            await _apiService.UploadGen2File(fileMetadata, ProjectAcronym.ToLower(), (uploadedBytes) =>
            {
                fileMetadata.uploadedBytes = uploadedBytes;
                StateHasChanged();
            });

            _uploadingFiles.Remove(fileMetadata);
            _files.RemoveAll(f => f.name == fileMetadata.name);
            _files.Add(fileMetadata);

            StateHasChanged();
        });

        StateHasChanged();
    }
    
    
    private string GetDirectoryName(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.Contains("/"))
            return string.Empty;

        var lastIndex = path.TrimEnd('/').LastIndexOf("/", StringComparison.Ordinal);
        return lastIndex == -1 ? "/" : path[..lastIndex] + "/";
    }

    private string GetFileName(string path)
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
        foreach (var browserFile in e.GetMultipleFiles())
        {
            await UploadFile(browserFile);
        }
    }
}