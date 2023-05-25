
using SpeedTestSharp.Client;
using SpeedTestSharp.Enums;

namespace Datahub.Desktop.Uploader;

public partial class SpeedTestPage : ContentPage
{
	public SpeedTestPage()
	{
		InitializeComponent();
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
            SkipBtn.IsEnabled = false;
            SpeedTestActivity.IsRunning = true;
            ISpeedTestClient speedTestClient = new SpeedTestClient();
            var result = await speedTestClient.TestSpeedAsync(SpeedUnit.Mbps);
            (Application.Current as App).Context.UploadSpeedMpbs = result.UploadSpeed;
            (Application.Current as App).Context.DownloadSpeedMpbs = result.DownloadSpeed;
            SpeedTestResultLb.Text = $"Download: {result.DownloadSpeed:0.#} {result.SpeedUnit} / Upload: {result.UploadSpeed:0.#} {result.SpeedUnit}";
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