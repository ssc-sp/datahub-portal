using Azure.Storage.Blobs;
using Microsoft.Maui.Platform;
using Datahub.Maui.Uploader;
using Datahub.Core.DataTransfers;
using Microsoft.Extensions.Localization;
using Datahub.Maui.Uploader.Models;
using Datahub.Maui.Uploader.IO;
using System.Threading;
using Windows.Storage.Pickers;
using CommunityToolkit.Maui.Storage;

namespace Datahub.Maui.Uploader
{

    public class LocalItemInfo
    {
        public LocalItemInfo(string name, bool isDirectory, long? bytes, string status) {
            this.name = name;
            this.isDirectory = isDirectory;
            this.bytes = bytes;
            this.status = status;
        }
        public string name { get; set; }
        public bool isDirectory { get; set; }
        public long? bytes { get; set; }
        public string status { get; set; }
    }
    public partial class UploadPage : ContentPage
    {

        public UploadPage(IStringLocalizer<App> localizer, DataHubModel dataHubModel,
            FileUtils fileUtils, IFolderPicker folderPicker)
        {
            InitializeComponent();
            this.localizer = localizer;
            this.dataHubModel = dataHubModel;
            this.fileUtils = fileUtils;
            this.folderPicker = folderPicker;
        }

        private List<LocalItemInfo> uploadList = new();
        private bool uploadInProgress;
        private UploadCredentials credentials;
        private readonly IStringLocalizer<App> localizer;
        private readonly DataHubModel dataHubModel;
        private readonly FileUtils fileUtils;
        private readonly IFolderPicker folderPicker;

        private void ContentPage_Loaded(object? sender, EventArgs e)
        {
            if (Handler?.MauiContext != null)
            {
                var uiElement = this.ToPlatform(Handler.MauiContext);
                DragDropHelper.RegisterDragDrop(uiElement, async fname =>
                {
                    if (!uploadList.Any(item => item.name == fname))
                    {
                        uploadList.Add(new (fname, Directory.Exists(fname), null, DEFAULT_STATUS));
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
                    FileListSection.Add(new TextCell() { Text = item.name, Detail = item.status });
                }
                LbUpload1.IsVisible = false;
                FileListTbView.IsVisible = true;
                UploadBtn.IsEnabled = true && !uploadInProgress;
            } else
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
                PickerTitle = "Please select a file to upload",                                
            };
            var result = await FilePicker.Default.PickMultipleAsync(options);
            if (result != null)
            {
                foreach (var file in result)
                {
                    uploadList.Add(new(file.FullPath, false, null, DEFAULT_STATUS));                    
                }
                await UpdateFileLayout();
            }

        }

        private async void OpenWSBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Uri uri = new Uri($"https://federal-science-datahub.canada.ca/w/{credentials.WorkspaceCode}/filelist");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
            }

        }

        public BlobContainerClient GetBlobServiceClient()
        {
            BlobContainerClient client = new(
                new Uri($"{dataHubModel.Credentials.SASToken}"),
                null);

            return client;
        }

        private async void UploadBtn_Clicked_AzNative(object sender, EventArgs e)
        {
            var client = GetBlobServiceClient();
            uploadInProgress = true;
            UploadBtn.IsEnabled = false;
            UploadProgressBar.IsVisible = true;
            
            //calculate file size
            foreach (var item in uploadList.Where(u => u.isDirectory))
            {
                item.status = "Determining directory size...";
            }
            foreach (var item in uploadList.Where(u => !u.isDirectory))
            {
                item.status = "Determining file size...";
            }
            await UpdateFileLayout();
            foreach (var item in uploadList.Where(u => u.isDirectory))
            {
                item.bytes = fileUtils.GetDirectorySize(item.name);
            }
            foreach (var item in uploadList.Where(u => !u.isDirectory))
            {
                item.bytes = new FileInfo(item.name).Length;
            }
            foreach (var item in uploadList)
            {
                item.status = "In Progress";
                await UpdateFileLayout();
                await Uploader.UploadBlocksAsync(client, item.name, 4096*10, async p => { UploadProgressBar.Progress = p; });
                item.status = "Completed";
                await UpdateFileLayout();
            }
            UploadBtn.IsEnabled = true;
            UploadProgressBar.IsVisible = false;
            uploadInProgress = true;

        }

        private async void AddfolderBtn_Clicked(object sender, EventArgs e)
        {
            var result = await folderPicker.PickAsync(CancellationToken.None);
            if (result.IsSuccessful)
            {
                await Toast.Make($"The folder was picked: Name - {result.Folder.Name}, Path - {result.Folder.Path}", ToastDuration.Long).Show(cancellationToken);
            }
            else
            {
                await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
            }
        }
    }
}
