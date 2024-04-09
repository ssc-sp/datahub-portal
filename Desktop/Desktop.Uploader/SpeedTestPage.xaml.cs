
using SpeedTestSharp.Client;
using SpeedTestSharp.Enums;
using Datahub.Maui.Uploader.Resources;

namespace Datahub.Maui.Uploader;

public partial class SpeedTestPage : ContentPage
{
    private readonly SpeedTestResults speedTestResults;

    public SpeedTestPage(SpeedTestResults speedTestResults)
	{
		InitializeComponent();
        this.speedTestResults = speedTestResults;
    }

    private async void SkipBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Upload");
    }

    private async void StartSpeedTestBtn_Clicked(object sender, EventArgs e)
    {
        StartSpeedTestBtn.IsEnabled = false;
        try
        {
            SpeedTestActivity.IsVisible = true;
            SpeedTestResultLb.Text = "Please be patient - we are testing your upload speed";
            SkipBtn.IsEnabled = false;
            SpeedTestActivity.IsRunning = true;
            ISpeedTestClient speedTestClient = new SpeedTestClient();
            var result = await speedTestClient.TestSpeedAsync(SpeedUnit.Mbps,testDownload:false,testLatency:false);
            speedTestResults.UploadSpeedMbps = result.UploadSpeed;
            SkipBtn.Text = "Continue";
            speedTestResults.DownloadSpeedMpbs = result.DownloadSpeed;
            Preferences.Set(nameof(result.UploadSpeed), result.UploadSpeed);
            Preferences.Set(nameof(result.DownloadSpeed), result.DownloadSpeed);
            //SpeedTestResultLb.Text = $"{AppResources.Download} {result.DownloadSpeed:0.#} {result.SpeedUnit} / {AppResources.Upload} {result.UploadSpeed:0.#} {result.SpeedUnit}";
            SpeedTestResultLb.Text = $"{AppResources.Upload} {result.UploadSpeed:0.#} {result.SpeedUnit}";


            //SpeedTestResultLb.Text = $"Download: {result.DownloadSpeed:0.#} {result.SpeedUnit} / Upload: {result.UploadSpeed:0.#} {result.SpeedUnit}";
            //SpeedTestResultLb.Text = $"Upload: {result.UploadSpeed:0.#} {result.SpeedUnit}";
            //var result = await speedTestClient.TestSpeedAsync(SpeedUnit.Mbps);
            //(Application.Current as App).Context.UploadSpeedMpbs = result.UploadSpeed;
            //(Application.Current as App).Context.DownloadSpeedMpbs = result.DownloadSpeed;
        }
        finally
        {
            SkipBtn.IsEnabled = true;
            SpeedTestActivity.IsRunning = false;
            SpeedTestActivity.IsVisible = false;
            StartSpeedTestBtn.IsEnabled = true;
        }
    }
}
