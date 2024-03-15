using Azure.Storage.Blobs;
using Microsoft.Maui.Platform;
using Datahub.Maui.Uploader.Models;
using Datahub.Maui.Uploader.IO;
using CommunityToolkit.Maui.Storage;
using Datahub.Maui.Uploader.Resources;

namespace Datahub.Maui.Uploader
{

	public class LocalItemInfo
    {
        public LocalItemInfo(string name, bool isDirectory, long? bytes, string status)
        {
            this.Name = name;
            this.IsDirectory = isDirectory;
            this.Bytes = bytes;
            this.Status = status;
        }
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public long? Bytes { get; set; }
        public string Status { get; set; }
        public int FileCount { get; set; } = 1;
        public List<FileInfo>? AllFiles { get; set; }

        public int FilesUploaded { get; set; } = 0;
        public bool IsUploaded => FilesUploaded == FileCount;
    }
    public partial class UploadPage : ContentPage
    {

        private CancellationTokenSource _cancellationTokenSource;

        public UploadPage(DataHubModel dataHubModel, SpeedTestResults speedTestResults,
            FileUtils fileUtils, IFolderPicker folderPicker)
        {
            InitializeComponent();
            this.dataHubModel = dataHubModel;
            this.speedTestResults = speedTestResults;
            this.fileUtils = fileUtils;
            this.folderPicker = folderPicker;
            currentSpeedMbps = speedTestResults.UploadSpeedMbps ?? 0;
            currentBlockSize = EstimateBlockSize();
            StorageURL = $"https://federal-science-datahub.canada.ca/w/{dataHubModel.Credentials.WorkspaceCode}/filelist";
            _cancellationTokenSource = new CancellationTokenSource();
            this.BindingContext = this;
        }

        private List<LocalItemInfo> uploadList = new();
        private readonly DataHubModel dataHubModel;
        private readonly SpeedTestResults speedTestResults;
        private readonly FileUtils fileUtils;
        private readonly IFolderPicker folderPicker;
        private double currentSpeedMbps;
        private int currentBlockSize;

        private void ContentPage_Loaded(object? sender, EventArgs e)
        {
            if (Handler?.MauiContext != null)
            {
                var uiElement = this.ToPlatform(Handler.MauiContext);
                DragDropHelper.RegisterDragDrop(uiElement, async fname =>
                {
                    if (!uploadList.Any(item => item.Name == fname))
                    {
                        uploadList.Add(new(fname, Directory.Exists(fname), null, AppResources.ReadyTxt));
                    }
                    await UpdateFileLayout();
                    //await mainPageViewModel.OpenFile(stream, CancellationToken.None);
                });
            }
        }

        private async Task UpdateFileLayout()
        {
            if (uploadList.Count > 0)
            {

                FileListSection.Clear();
                foreach (var item in uploadList)
                {
                    FileListSection.Add(new TextCell() { Text = item.Name, Detail = item.Status });
                }
                LbUpload1.IsVisible = false;
                FileListTbView.IsVisible = true;
                UploadBtn.IsEnabled = uploadList.Count(e => !e.IsUploaded) > 0;
            }
            else
            {
                LbUpload1.IsVisible = true;
                FileListTbView.IsVisible = false;
                UploadBtn.IsEnabled = false;
            }
        }

        public const string DEFAULT_STATUS = "Ready...";

        private async void AddfileBtn_Clicked(object sender, EventArgs e)
        {
            PickOptions options = new()
            {
                PickerTitle = AppResources.SelectFilePicker,
                
            };
            var result = await FilePicker.Default.PickMultipleAsync(options);
            if (result != null)
            {
                foreach (var file in result)
                {
                    var item = new LocalItemInfo(file.FullPath, false, null, DEFAULT_STATUS);
                    uploadList.Add(item);
                    item.Bytes = new FileInfo(item.Name).Length;
                    item.Status = $"{fileUtils.GetFriendlyFileSize(item.Bytes.Value)}";
                }
                await UpdateFileLayout();
            }

        }

        private async void OpenWSBtn_Clicked(object sender, EventArgs e)
        {
            await OpenBrowserStorage();
        }

        private async Task OpenBrowserStorage()
        {
            try
            {
                Uri uri = new Uri($"https://federal-science-datahub.canada.ca/w/{dataHubModel.Credentials.WorkspaceCode}/filelist");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
            }
        }

        public string StorageURL { get; init; }

        //private async void OpenWSBtn_Clicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        await Browser.Default.OpenAsync(new Uri(StorageURL), BrowserLaunchMode.SystemPreferred);
        //    }
        //    catch (Exception ex)
        //    {
        //        // An unexpected error occurred. No browser may be installed on the device.
        //    }

        //}

        public BlobContainerClient GetBlobServiceClient()
        {
            if (dataHubModel?.Credentials is null) throw new InvalidNavigationException("No credentials available");
            BlobContainerClient client = new(
                new Uri($"{dataHubModel.Credentials.SASToken}"),
                null);

            return client;
        }

        private async void UploadBtn_Clicked(object sender, EventArgs e)
        {
            var client = GetBlobServiceClient();
            UploadBtn.IsVisible = false;
            AddfilesSection.IsVisible = false;
            CancelBtn.IsVisible = true;
            UploadProgressBar.IsVisible = true;
            LbUploadStatus.IsVisible = true;
            prevProgressValue = null;
            prevProgressTS = null;
            cts = new CancellationTokenSource();
            currentUploadBatch = uploadList.Where(e => !e.IsUploaded).ToList();
            currentUploadBatch.ForEach(e => e.FilesUploaded = 0);
            foreach (var item in currentUploadBatch)
            {
                item.Status = AppResources.InProgress;
                await UpdateFileLayout();
                var success = false;
                if (item.IsDirectory)
                {
                    success = await UploadDirectory(client, item);
                }
                else
                {
                    success = await UploadFile(client, item);
                }
                if (success)
                    item.Status = "Completed";
                else
                    item.Status = "Failed";
                await UpdateFileLayout();
            }
            await OpenBrowserStorage();
            UploadBtn.IsVisible = true;
            UploadProgressBar.IsVisible = false;
            LbUploadStatus.IsVisible = false;
            CancelBtn.IsVisible = false;
            AddfilesSection.IsVisible = true;
            uploadList.RemoveAll(e => e.IsUploaded);
            await UpdateFileLayout();
        }

        private DateTime? prevProgressTS;
        private double? prevProgressValue;
        private CancellationTokenSource cts;
        private List<LocalItemInfo> currentUploadBatch;

        private async Task UpdateProgressStatus(double progress, long? currentSize)
        {
            if (prevProgressTS.HasValue && currentSize.HasValue)
            {
                var mbytesSinceLastUpdate = (progress - prevProgressValue.Value) * currentSize.Value / (1024 * 1024);
                var secsSinceLastUpdate = (DateTime.Now - prevProgressTS.Value).TotalSeconds;
                var instantSpeed = Math.Min(0,mbytesSinceLastUpdate / secsSinceLastUpdate);
                currentSpeedMbps = (instantSpeed * 0.5 + currentSpeedMbps * 1.5) / 2;
            }
            var totalUploadSize = currentUploadBatch.Sum(i => i.Bytes ?? 0);
            var uploaded = currentUploadBatch.Where(i => i.IsUploaded).Sum(i => i.Bytes ?? 0);
            var totalProgress = uploaded * 1.0 / totalUploadSize;
            var maxProgressCurrent = currentSize / totalUploadSize;
            var currentProgress = totalProgress + progress * ((currentSize ?? 0) * 1.0 / totalUploadSize);
            UploadProgressBar.Progress = currentProgress;
            var totalFiles = currentUploadBatch.Sum(i => i.AllFiles?.Count ?? 1);
            var filesCompleted = currentUploadBatch.Sum(i => i.FilesUploaded);
            if (currentSpeedMbps > 0)
            {
                var remaining = totalUploadSize - (uploaded + (currentSize ?? 0) * progress);
                var seconds = remaining * 1.0 / 1024 / 1000 / currentSpeedMbps;
                var ts = TimeSpan.FromSeconds(seconds);
                var friendly = fileUtils.ToFriendlyFormat(ts);
                LbUploadStatus.Text = $"{currentProgress * 100:F2} % - {friendly} remaining - {currentSpeedMbps:F2} Mbps - {filesCompleted}/{totalFiles} files";
            }
            else
            {
                LbUploadStatus.Text = $"{currentProgress * 100:F2} % - {filesCompleted}/{totalFiles} files";
            }
            prevProgressTS = DateTime.Now;
            prevProgressValue = progress;
        }

        private async Task UpdateBlockSize(int updatedBlockSize)
        {
            currentBlockSize = updatedBlockSize;
        }

        private async Task<bool> UploadFile(BlobContainerClient client, LocalItemInfo item)
        {
            var result = await Uploader.UploadBlocksAsync(client, item.Name, Path.GetFileName(item.Name), currentBlockSize, EstimateBlockSize(), async p => await UpdateProgressStatus(p, item.Bytes), UpdateBlockSize, cts.Token);
            if (result) item.FilesUploaded = 1;
            return result;
        }

        private async Task<bool> UploadDirectory(BlobContainerClient client, LocalItemInfo item)
        {
            long uploadedBytes = 0;
            var basePath = new DirectoryInfo(item.Name).Parent;
            var success = true;
            foreach (var file in item.AllFiles)
            {
                var totalProgress = uploadedBytes * 1.0 / item.Bytes.Value;                
                var maxProgress = (uploadedBytes + file.Length) * 1.0 / item.Bytes.Value;
                var relPath = Path.GetRelativePath(basePath.FullName, file.FullName);
                var blobPath = relPath.Replace('\\', '/');
                var isUploaded = await Uploader.UploadBlocksAsync(client, file.FullName, blobPath, currentBlockSize,
                    EstimateBlockSize(), async p => await UpdateProgressStatus(totalProgress + p * (file.Length * 1.0 / item.Bytes.Value), item.Bytes.Value), UpdateBlockSize, cts.Token);
                success = success && isUploaded;
                uploadedBytes = uploadedBytes + file.Length;
                if (cts.IsCancellationRequested) { return false; }
                if (isUploaded) item.FilesUploaded++;
            }
            return success;
        }

        private int EstimateBlockSize()
        {
            //https://stackoverflow.com/questions/44775415/how-to-choose-blob-block-size-in-azure
            //1Mb block size 
            //The block must be less than or equal to 4,000 mebibytes (MiB)
            return (int)Math.Min(4000, Math.Max(1, speedTestResults.UploadSpeedMbps ?? 0 / 8)) * 1000 * 1000;
        }

        private async void AddfolderBtn_Clicked(object sender, EventArgs e)
        {
            var result = await folderPicker.PickAsync(CancellationToken.None);
            if (result.IsSuccessful)
            {
                var item = new LocalItemInfo(result.Folder.Path, true, null, DEFAULT_STATUS);
                uploadList.Add(item);
                await Task.Factory.StartNew(() =>
                {
                    var dirInfo = fileUtils.GetDirectorySize(item.Name);
                    item.Bytes = dirInfo.bytes;
                    item.AllFiles = dirInfo.allFiles;
                    item.FileCount = dirInfo.files;
                    item.Status = $"{item.FileCount} files / {fileUtils.GetFriendlyFileSize(item.Bytes.Value)}";
                    MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await UpdateFileLayout();
                    });
                });
                //await Toast.Make($"The folder was picked: Name - {result.Folder.Name}, Path - {result.Folder.Path}", ToastDuration.Long).Show(CancellationToken.None);
            }
            //else
            //{
            //    await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(CancellationToken.None);
            //}
        }

        private void CancelBtn_Clicked(object sender, EventArgs e)
        {
            cts.Cancel();
        }
    }
}
