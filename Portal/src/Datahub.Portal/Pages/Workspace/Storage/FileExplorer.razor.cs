using Datahub.Application.Exceptions;
using Datahub.Application.Services.Publishing;
using Datahub.Core.Data;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Datahub;
using Datahub.Portal.Layout;
using Datahub.Portal.Pages.Workspace.Publishing;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Datahub.Portal.Pages.Workspace.Storage;

public partial class FileExplorer
{
    private const string DefaultUploadFolderName = "upload";
    private async Task RefreshStoragePageAsync()
    {
        _lastContainer = Container;
        _loading = true;
        StateHasChanged();

        var dfsPage = await StorageManager.GetDfsPagesAsync(Container.Name, _currentFolder, _continuationToken);

        _continuationToken = dfsPage.ContinuationToken;
        _files = dfsPage.Files;
        _folders = dfsPage.Folders;

        _loading = false;

        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_lastContainer != Container)
        {
            try
            {
                await EnsureDefaultUploadFolderAsync();
                await SetCurrentFolder(_root);
            }
            catch (Exception e)
            {
                _failed = true;
                _logger.LogError(e, "Failed to load file explorer");
            }
        }
    }

    private async Task EnsureDefaultUploadFolderAsync()
    {
        if (StorageManager is null || string.IsNullOrWhiteSpace(ContainerName))
            return;

        await StorageManager.CreateFolderAsync(ContainerName, _root, DefaultUploadFolderName);
    }

    private async Task HandleNewFolder(string folderName)
    {
        var newFolderName = folderName.Replace("/", "").Replace("\\", "").Trim();
        var newFolderPath = JoinPath(_currentFolder, newFolderName);
        if (_folders.Contains(newFolderPath))
            return;

        if (!await StorageManager.CreateFolderAsync(ContainerName, _currentFolder, newFolderName))
            return;

        _folders.Add(newFolderPath);

        _folderList = await GetFileCountAsync("/");

        await _telemetryService.LogTelemetryEvent(TelemetryEvents.UserCreateFolder);
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

        
        foreach (var selectedFile in toBeDeleted)
        {
            if (!await StorageManager.DeleteFileAsync(ContainerName, JoinPath(_currentFolder, selectedFile)))
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

        if (!await StorageManager.RenameFileAsync(ContainerName, oldFileName, newFileName))
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

        if (!await StorageManager.RenameFileAsync(ContainerName, oldFileName, newFileName))
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

        if (!await StorageManager.DeleteFolderAsync(ContainerName, folderName))
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


    private async Task<(bool FileExists, bool AllowOverride)> VerifyOverwrite(string filePath)
    {
        if (!await StorageManager.FileExistsAsync(ContainerName, filePath))
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

    private async Task UploadFile(IBrowserFile browserFile, string folder)
    {
        if (browserFile == null)
            return;

        folder = DefaultUploadFolderName;
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
            createdby = GraphUser.Mail,
            folderpath = folder,
            filename = browserFile.Name,
            filesize = browserFile.Size.ToString(),
            uploadStatus = FileUploadStatus.SelectedToUpload,
            bytesToUpload = browserFile.Size,
            createdts = DateTime.UtcNow,
            lastmodifiedts = DateTime.UtcNow,
            BrowserFile = browserFile
        };

        lock (this)
        {
            _uploadingFiles.Add(fileMetadata);
        }

        _ = InvokeAsync(async () =>
        {
            var succeeded = await StorageManager.UploadFileAsync(ContainerName, fileMetadata, uploadedBytes =>
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
                }
            }
            else if (succeeded)
            {
                await SetCurrentFolder(DefaultUploadFolderName);
            }

            await InvokeAsync(StateHasChanged);
        });
    }

    private async Task HandleFileDownload(string filename)
    {
        try
        {
            var uri = await StorageManager.DownloadFileAsync(ContainerName, JoinPath(_currentFolder, filename));
            await _module.InvokeVoidAsync("downloadFile", uri.ToString());
        }
        catch (UnauthorizedAccessException ex)
        {
            // File is quarantined - show user-friendly message
            _snackbar.Add($"File access denied: {filename}. The file is currently quarantined and awaiting virus scan completion. Please try again later.", Severity.Warning);
            _logger.LogWarning("Download blocked for quarantined file: {FileName}. {Message}", filename, ex.Message);
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Failed to download file: {filename}", Severity.Error);
            _logger.LogError(ex, "Error downloading file: {FileName}", filename);
        }
    }

    private async Task HandlePublishFiles(IEnumerable<FileMetaData> files)
    {
        if (!_config.CkanConfiguration.IsFeatureEnabled)
        {
            await Task.CompletedTask;
            return;
        }

        var dialogParams = new DialogParameters<PublishNewDatasetDialog>
        {
            { x => x.IsFileExplorerDialog, true },
            { x => x.WorkspaceId, ProjectId },
            { x => x.Files, files }
        };

        var options = new DialogOptions() { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseOnEscapeKey = true };

        var dialog =
            await _dialogService.ShowAsync<PublishNewDatasetDialog>(Localizer["Add Files To Dataset"], dialogParams,
                options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            // if adding to an existing submission, result.Data will have that submission
            var submission = result.Data as OpenDataSubmission;
            if (submission == null)
            {
                // if creating a new one, result.Data will have the basic info to create it
                var submissionInfo = result.Data as OpenDataSubmissionBasicInfo;
                if (submissionInfo == null)
                {
                    throw new OpenDataPublishingException("Could not get submission information");
                }

                submission = await _publishingService.CreateOpenDataSubmission(submissionInfo);
            }

            // if it's still null here, something has gone wrong
            if (submission == null)
            {
                throw new OpenDataPublishingException("No available submission provided or created");
            }

            await _publishingService.AddFilesToSubmission(submission, files, Container.Id, ContainerName);

            _navManager.NavigateTo(
                $"/{PageRoutes.WorkspacePrefix}/{ProjectAcronym}/{WorkspaceSidebar.SectionViews.Publishing}/{submission.Id}");
        }

        await Task.CompletedTask;
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
}