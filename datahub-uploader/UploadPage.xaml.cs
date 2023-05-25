﻿using Azure.Storage.Blobs;
using Datahub.Desktop.Uploader.Platforms;
using Microsoft.Maui.Platform;

namespace Datahub.Desktop.Uploader
{
    public partial class UploadPage : ContentPage
    {

        public UploadPage()
        {
            InitializeComponent();
        }

        private Dictionary<string,string> fileList = new();
        private bool uploadInProgress;

        private void ContentPage_Loaded(object? sender, EventArgs e)
        {
            if (Handler?.MauiContext != null)
            {
                var uiElement = this.ToPlatform(Handler.MauiContext);
                DragDropHelper.RegisterDragDrop(uiElement, async fname =>
                {
                    if (!fileList.ContainsKey(fname))
                    {
                        fileList.Add(fname, "Ready...");
                    }
                    await UpdateFileLayout();
                    //await mainPageViewModel.OpenFile(stream, CancellationToken.None);
                });
            }
        }

        private async Task UpdateFileLayout()
        {
            if (fileList.Count > 0)
            {
                
                FileListSection.Clear();
                foreach (var item in fileList)
                {
                    FileListSection.Add(new TextCell() { Text = item.Key, Detail = item.Value });
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
                    fileList.Add(file.FullPath,"Ready...");
                }
                await UpdateFileLayout();
            }

        }

        private async void OpenWSBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Uri uri = new Uri("https://federal-science-datahub.canada.ca/w/DW1/filelist");
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
                new Uri($"key"),
                null);

            return client;
        }

        private async void UploadBtn_Clicked(object sender, EventArgs e)
        {
            var client = GetBlobServiceClient();
            uploadInProgress = true;
            UploadBtn.IsEnabled = false;
            UploadProgressBar.IsVisible = true;
            foreach (var item in fileList)
            {
                fileList[item.Key] = "In Progress";
                await UpdateFileLayout();
                await Uploader.UploadBlocksAsync(client, item.Key, 4096, async p => { UploadProgressBar.Progress = p; });
                fileList[item.Key] = "Completed";
                await UpdateFileLayout();
            }
            UploadBtn.IsEnabled = true;
            UploadProgressBar.IsVisible = false;
            uploadInProgress = true;

        }
    }
}