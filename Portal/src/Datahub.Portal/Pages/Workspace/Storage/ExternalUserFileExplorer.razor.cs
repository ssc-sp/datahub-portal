using Datahub.Application.Exceptions;
using Datahub.Core.Data;
using Datahub.Core.Model;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Storage;

public partial class ExternalUserFileExplorer
{
    private string? _lastContainerName;

    private async Task LoadContainersAsync()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            _availableContainers = await StorageManager.GetContainersAsync();
            _showingContainers = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load containers");
            _availableContainers = new List<string>();
        }

        _loading = false;
        StateHasChanged();
    }

    private async Task SelectContainer(string containerName)
    {
        _selectedContainerName = containerName;
        _showingContainers = false;
        _currentFolder = _root;
        
        // Load metadata for the selected container
        StorageAccountMetadata = await StorageManager.GetStorageMetadataAsync(containerName);
        _folderList = await GetFileCountAsync(_currentFolder);
        
        await RefreshStoragePageAsync();
    }

    private void ToggleContainerSelection(string containerName)
    {
        if (_selectedItems.Contains(containerName))
        {
            _selectedItems.Remove(containerName);
        }
        else
        {
            _selectedItems = new HashSet<string> { containerName };
        }
    }

    private async Task BackToContainers()
    {
        _showingContainers = true;
        _selectedContainerName = null;
        _currentFolder = _root;
        _selectedItems = new HashSet<string>();
        _files = new List<FileMetaData>();
        _folders = new List<string>();
        StateHasChanged();
    }

    private async Task RefreshStoragePageAsync()
    {
        _lastContainerName = Container?.Name;
        _loading = true;
        StateHasChanged();

        var containerName = _selectedContainerName ?? Container.Name;
        var dfsPage = await StorageManager.GetDfsPagesAsync(containerName, _currentFolder, _continuationToken);

        _continuationToken = dfsPage.ContinuationToken;
        _files = dfsPage.Files;
        _folders = dfsPage.Folders;

        // Load scan results for all files
        var fileNames = _files.Select(f => f.name).ToList();
        _fileScanResults = await FileScanService.GetFileScanResultsAsync(fileNames);

        _loading = false;

        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_lastContainerName != Container?.Name)
        {
            try
            {
                await LoadContainersAsync();
            }
            catch (Exception e)
            {
                _failed = true;
                _logger.LogError(e, "Failed to load file explorer");
            }
        }
    }

    private async Task HandleFilesDelete(string fileName)
    {
        // If a single filename is provided, add it to the selected items
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            _selectedItems.Add(fileName);
        }
        var toBeDeleted = _selectedItems.Where(x => x != _currentFolder).ToList();

        // Ensure there are selected items to delete
        if (toBeDeleted == null || !toBeDeleted.Any())
            return;

        var fileCount = toBeDeleted.Count;
        var message = string.Format(Localizer["You are about to delete {0} files. Are you sure?"], fileCount);
        if (fileCount == 1)
        {
            message = string.Format(Localizer["Are you sure you want to delete file \"{0}\"?"].ToString(), fileName);
        }
        if (!await _jsRuntime.InvokeAsync<bool>("confirm", message))
            return;

        var containerName = _selectedContainerName ?? ContainerName;
        foreach (var selectedFile in toBeDeleted)
        {
            if (!await StorageManager.DeleteFileAsync(containerName, JoinPath(_currentFolder, selectedFile)))
                continue;

            _files?.RemoveAll(f => f.name.Equals(selectedFile, StringComparison.OrdinalIgnoreCase));
        }

        // Clear selected items and reset to the current folder
        _selectedItems = new HashSet<string> { _currentFolder };
        await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserDeleteFile);
    }


    private async Task HandleFileItemDrop(string folder, string fileName)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName))
            return;

        var oldFileName = JoinPath(_currentFolder, fileName);
        var newFileName = JoinPath(folder, fileName);

        var (_, allowOverride) = await VerifyOverwrite(newFileName);
        if (!allowOverride)
            return;

        var containerName = _selectedContainerName ?? ContainerName;
        if (!await StorageManager.RenameFileAsync(containerName, oldFileName, newFileName))
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

        var (fileExists, allowOverride) = await VerifyOverwrite(newFileName);
        if (!allowOverride)
            return;

        var containerName = _selectedContainerName ?? ContainerName;
        if (!await StorageManager.RenameFileAsync(containerName, oldFileName, newFileName))
            return;

        if (fileExists)
        {
            _files.RemoveAll(f => f.name == fileRename);
        }

        var targetFile = _files.FirstOrDefault(f => f.name == currentFileName);
        if (targetFile is not null)
            targetFile.name = fileRename;
    }

    private async Task HandleDeleteFolder()
    {
        var folderName = _selectedItems?.FirstOrDefault();
        if (folderName is null)
        {
            folderName = _currentFolder;
        }

        if (folderName.Length < _currentFolder.Length)  // delete from inside folder
        {
            folderName = _currentFolder;
        }

        var folderNameOnly = folderName.TrimEnd('/').Split('/').Last();

        var message = string.Format(Localizer["Are you sure you want to delete folder \"{0}\"?"], folderNameOnly);
      
        if (!await _jsRuntime.InvokeAsync<bool>("confirm", message))
            return;

        var containerName = _selectedContainerName ?? ContainerName;
        if (!await StorageManager.DeleteFolderAsync(containerName, folderName))
            return;

        if (folderName == _currentFolder)
        {
            await SetCurrentFolder(GetDirectoryName(_currentFolder));
        }
        else
        {
            _folders.Remove(folderName);
        }
        StateHasChanged();
    }

    private Task HandlePublishFiles(IEnumerable<FileMetaData> files)
    {
        // External users cannot publish files
        return Task.CompletedTask;
    }

    private async Task<(bool FileExists, bool AllowOverride)> VerifyOverwrite(string filePath)
    {
        var containerName = _selectedContainerName ?? ContainerName;
        if (!await StorageManager.FileExistsAsync(containerName, filePath))
            return (false, true);

        var allowOverride = await _jsRuntime.InvokeAsync<bool>("confirm",
            string.Format(Localizer["File '{0}' already exists. Do you want to overwrite it?"], filePath));

        return (true, allowOverride);
    }

    private string JoinPath(string folder, string fileName)
    {
        var splitPath = (folder ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        splitPath.Add(fileName);
        return string.Join("/", splitPath);
    }

    private bool CheckAcceptedFileExtension(IBrowserFile browserFile)
    {
        var blockedExtensions = _config.StorageConfiguration.BlockedFileExtensionCollection;
        var filename = browserFile.Name;
        var extension = Path.GetExtension(filename)?.ToLowerInvariant();

        return !blockedExtensions.Contains(extension);
    }

    private static string GenerateUploadBatchId() => Guid.NewGuid().ToString();

    private async Task UploadFile(IBrowserFile browserFile, string folder, string uploadBatchId)
    {
        if (browserFile == null)
            return;

        var isAccepted = CheckAcceptedFileExtension(browserFile);
        if (!isAccepted)
        {
            _blockedFiles.Add(browserFile.Name);
            return;
        }

        var newFilePath = JoinPath(folder, browserFile.Name);

        var (fileExists, allowOverride) = await VerifyOverwrite(newFilePath);
        if (!allowOverride)
            return;

        var fileMetadata = new FileMetaData
        {
            id = Guid.NewGuid().ToString(),
            createdby = PortalUser.Email,
            folderpath = folder,
            filename = browserFile.Name,
            filesize = browserFile.Size.ToString(),
            uploadStatus = FileUploadStatus.SelectedToUpload,
            bytesToUpload = browserFile.Size,
            createdts = DateTime.UtcNow,
            lastmodifiedts = DateTime.UtcNow,
            uploadBatchId = uploadBatchId,
            BrowserFile = browserFile
        };

        lock (this)
        {
            _uploadingFiles.Add(fileMetadata);
        }

        var containerName = _selectedContainerName ?? ContainerName;
        _ = InvokeAsync(async () =>
        {
            var succeeded = await StorageManager.UploadFileAsync(containerName, fileMetadata, uploadedBytes =>
            {
                fileMetadata.uploadedBytes = uploadedBytes;
                _ = InvokeAsync(StateHasChanged);
            });

            lock (this)
            {
                _uploadingFiles.Remove(fileMetadata);
                if (!_uploadingFiles.Any())
                {
                    _uploadingFiles = new();
                }
            }

            if (folder == _currentFolder)
            {
                if (succeeded)
                {
                    if (fileExists)
                    {
                        _files.RemoveAll(f => f.name == fileMetadata.name);
                    }

                    _files.Add(fileMetadata);
                    
                    // Set initial scan status for newly uploaded file
                    _fileScanResults[fileMetadata.name] = new FileScanResult
                    {
                        FileName = fileMetadata.name,
                        Status = FileScanStatus.ScanInProgress,
                        ScanDate = DateTime.UtcNow
                    };
                }
            }

            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task HandleFileDownload(string filename)
    {
        var containerName = _selectedContainerName ?? ContainerName;
        var uri = await StorageManager.DownloadFileAsync(containerName, JoinPath(_currentFolder, filename));
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
        var uploadBatchId = GenerateUploadBatchId();

        foreach (var browserFile in e.GetMultipleFiles())
        {
            await UploadFile(browserFile, folderName, uploadBatchId);
        }

        await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserUploadFile);

        // refresh ui
        await InvokeAsync(StateHasChanged);
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

    private async Task ToggleSelectAllFiles()
    {
        _allFilesSelected = !_allFilesSelected;

        if (_allFilesSelected)
        {
            _selectedItems = _files.Select(f => f.name).ToHashSet();
        }
        else
        {
            _selectedItems.Clear();
        }
    }

    private bool CanDeleteFolder(string folderName)
    {
        if (_folders.Contains(folderName) && _folderList != null)
        {
            return _folderList.TryGetValue($"{folderName}/", out int fileCount) && fileCount == 0;
        }
        return false;
    }
}
