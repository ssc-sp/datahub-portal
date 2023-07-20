using Datahub.Core.DataTransfers;
using Datahub.Maui.Uploader.Models;
using Microsoft.Extensions.Localization;

namespace Datahub.Maui.Uploader
{
    public partial class ValidateCodePage : ContentPage
    {
        private readonly IStringLocalizer<App> localizer;
        private readonly DataHubModel model;

        public ValidateCodePage(IStringLocalizer<App> localizer, DataHubModel model)
        {
            InitializeComponent();
            this.localizer = localizer;
            this.model = model;
        }


        private void UploadCodeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCodeButton.IsEnabled = true;
        }

        private async void ValidateCodeButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//Upload");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {

        }

        private async void GetUploadCodeBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Uri uri = new Uri($"https://federal-science-datahub.canada.ca/w/{model.Credentials.WorkspaceCode}/filelist");
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
            }
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            Clipboard.Default.ClipboardContentChanged += Clipboard_ClipboardContentChanged;
        }

        private async void Clipboard_ClipboardContentChanged(object sender, EventArgs e)
        {
            if (Clipboard.Default.HasText)
            {
                var content = await Clipboard.Default.GetTextAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    var decoded = CredentialEncoder.DecodeCredentials(content);
                    if (decoded != null)
                    {
                        model.Credentials = decoded;
                        await Shell.Current.GoToAsync("//SpeedTest");
                    }
                }
            }
        }
    }
}
